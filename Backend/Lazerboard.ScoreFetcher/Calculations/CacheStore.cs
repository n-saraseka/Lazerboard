using System.Timers;
using Lazerboard.Data.Redis.Repositories.Interfaces;
using Lazerboard.ScoreFetcher.OsuApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using osu.Game.Beatmaps;
using osu.Game.IO;

namespace Lazerboard.ScoreFetcher.Calculations;

public class CacheStore : ICacheStore
{
    private readonly IServiceProvider _serviceProvider;
    private OsuApiService _osuApiService;
    private ILogger<CacheStore> _logger;
    private string _cachePath;
    private int _osuFileTtl;
    private const int DefaultTtl = 10;
    private const int MaxDownloadAttempts = 5;
    private double _apiInterval;
    private System.Timers.Timer _cleanupTimer;
    private readonly SemaphoreSlim _cleanupSemaphore = new(1, 1);

    public CacheStore(IConfiguration config, OsuApiService osuApiService, ILogger<CacheStore> logger, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _osuApiService = osuApiService;
        _logger = logger;
        
        var cacheConfig = config.GetSection("Caching");
        
        var currentDir = Directory.GetCurrentDirectory();
        _cachePath = $"{currentDir}/{cacheConfig["Folder"]}";
        if (!Directory.Exists(_cachePath))
        {
            Directory.CreateDirectory(_cachePath);
        }
        
        _osuFileTtl = int.TryParse(cacheConfig["FileTTL"], out var osuFileTtl) ? osuFileTtl : DefaultTtl;
        
        _cleanupTimer = new System.Timers.Timer(TimeSpan.FromMinutes(_osuFileTtl).TotalMilliseconds);
        _cleanupTimer.Elapsed += OnCleanupTimerActivated;
        _cleanupTimer.AutoReset = true;
        _cleanupTimer.Enabled = true;
        
        var apiConfig = config.GetSection("OsuApi");
        _apiInterval = apiConfig.GetValue<double>("ApiInterval");
    }

    private void OnCleanupTimerActivated(object? source, ElapsedEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!await _cleanupSemaphore.WaitAsync(0))
                return;
            try
            {
                await CleanupCacheAsync();
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception, "Failed to cleanup beatmap cache");
            }
            finally
            {
                _cleanupSemaphore.Release();
            }
        });
    }

    public async Task CleanupCacheAsync()
    {
        var deletedCount = 0;
        var files = Directory.EnumerateFiles(_cachePath).ToList();
        using var scope = _serviceProvider.CreateScope();
        var beatmapCacheRepository = scope.ServiceProvider.GetRequiredService<IBeatmapCacheRepository>();

        var beatmapIds = files.Select(f =>
        {
            int? beatmapId = null;
            if (int.TryParse(Path.GetFileNameWithoutExtension(f), out var id)) 
                beatmapId = id;
            return beatmapId;
        })
        .Where(beatmapId => beatmapId != null)
        .Select(beatmapId => (int)beatmapId!)
        .ToList();
        
        var cachedFileNames = await beatmapCacheRepository.GetCachedBeatmapFileNamesAsync(beatmapIds);
        
        foreach (var file in files)
        {
            var beatmapId = int.Parse(Path.GetFileNameWithoutExtension(file));
            if (cachedFileNames[beatmapId] is null)
            {
                File.Delete(file);
                deletedCount++;
            }
        }
        _logger.Log(LogLevel.Information, "Removed {count} files from beatmap cache", deletedCount);
    }
    
    public async Task<Beatmap> GetBeatmapFileAsync(int beatmapId, IBeatmapCacheRepository beatmapCacheRepository, CancellationToken ct)
    {
        var mapPath = $"{_cachePath}/{beatmapId}.osu";
        var attempts = 0;
        
        // Set / reset .osu file TTL in Redis
        var cachedFileName = await beatmapCacheRepository.GetCachedBeatmapFileNameAsync(beatmapId);
        if (cachedFileName is null)
        {
            await beatmapCacheRepository
                .SetCachedBeatmapFileNameAsync(beatmapId, $"{beatmapId}.osu",
                    TimeSpan.FromMinutes(_osuFileTtl));
        }
        else
        {
            await beatmapCacheRepository.ResetCachedBeatmapFileNameTtlAsync(beatmapId, TimeSpan.FromMinutes(_osuFileTtl));
        }

        while (attempts < MaxDownloadAttempts)
        {
            if (!File.Exists(mapPath))
            {
                try
                {
                    await _osuApiService.DownloadBeatmapAsync(beatmapId, ct);
                    await Task.Delay(TimeSpan.FromSeconds(_apiInterval), ct);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, ex, "Download failed for Beatmap ID {id}, attempt no. {attempt}", beatmapId, attempts);
                    attempts++;
                    continue;
                }
            }
            try
            {
                await using var stream = File.OpenRead(mapPath);
                using var reader = new LineBufferedReader(stream);
                return osu.Game.Beatmaps.Formats.Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
            }
            catch (FileNotFoundException ex)
            {
                _logger.Log(LogLevel.Error, ex, "Failed to get .osu file for beatmap ID {id}, attempt no. {attempt}", beatmapId, attempts);
                attempts++;
            }
        }
        
        throw new InvalidOperationException(
            $"Failed to get beatmap {beatmapId} after {MaxDownloadAttempts} attempts");
    }
}