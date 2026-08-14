using System.Timers;
using osu.Game.IO;
using osu.Game.Beatmaps;
using OsuScoreStats.OsuApi;

namespace OsuScoreStats.Calculations;

public class CacheStore : ICacheStore
{
    private OsuApiService _osuApiService;
    private ILogger<CacheStore> _logger;
    private string _cachePath;
    private int _osuFileTTL;
    private const int DefaultTTL = 10;
    private double _apiInterval;
    private System.Timers.Timer _cleanupTimer;

    public CacheStore(IConfiguration config, OsuApiService osuApiService, ILogger<CacheStore> logger)
    {
        _osuApiService = osuApiService;
        _logger = logger;
        
        var cacheConfig = config.GetSection("Caching");
        
        var currentDir = Directory.GetCurrentDirectory();
        _cachePath = $"{currentDir}/{cacheConfig["Folder"]}";
        if (!Directory.Exists(_cachePath))
        {
            Directory.CreateDirectory(_cachePath);
        }
        
        _osuFileTTL = int.TryParse(cacheConfig["osuFileTTL"], out var osuFileTTL) ? osuFileTTL : DefaultTTL;
        
        _cleanupTimer = new System.Timers.Timer(TimeSpan.FromMinutes(_osuFileTTL / 2).TotalMilliseconds);
        _cleanupTimer.Elapsed += OnCleanupTimerActivated;
        _cleanupTimer.AutoReset = true;
        _cleanupTimer.Enabled = true;
        
        var apiConfig = config.GetSection("OsuApi");
        _apiInterval = apiConfig.GetValue<double>("ApiInterval");
    }

    private void OnCleanupTimerActivated(object? source, ElapsedEventArgs e)
    {
        var deletedCount = 0;
        var files = Directory.EnumerateFiles(_cachePath);
        foreach (var file in files)
            if (DateTime.UtcNow - File.GetCreationTimeUtc(file) >= TimeSpan.FromMinutes(_osuFileTTL))
            {
                File.Delete(file);
                deletedCount++;
            }
        _logger.Log(LogLevel.Information, "Removed {count} files from beatmap cache", deletedCount);
    }
    
    public async Task<Beatmap> GetBeatmapFileAsync(int beatmapId, CancellationToken ct)
    {
        var mapPath = $"{_cachePath}/{beatmapId}.osu";
        if (!File.Exists(mapPath))
        {
            try
            {
                await _osuApiService.DownloadBeatmapAsync(beatmapId, ct);
                await Task.Delay(TimeSpan.FromSeconds(_apiInterval), ct);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, "Method: OsuApiService.DownloadBeatmapsAsync | BeatmapID: {id}", beatmapId);
            }
        }
        
        await using var stream = File.OpenRead(mapPath);
        using var reader = new LineBufferedReader(stream);
        return osu.Game.Beatmaps.Formats.Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
    }
}