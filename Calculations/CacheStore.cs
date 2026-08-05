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
    private TimeSpan _deltaTime;
    private DateTime _startTime;
    private double _apiInterval;

    public CacheStore(IConfiguration config, OsuApiService osuApiService, ILogger<CacheStore> logger)
    {
        _osuApiService = osuApiService;
        _logger = logger;
        _cachePath = config.GetValue<string>("CacheFolder");
        _osuFileTTL = int.TryParse(config["osuFileTTL"], out var osuFileTTL) ? osuFileTTL : DefaultTTL;
        _deltaTime = TimeSpan.Zero;
        _startTime = DateTime.UtcNow;
        _apiInterval = config.GetValue<double>("OsuApiInterval");
    }

    private bool ShouldCleanUp()
    {
        _deltaTime += DateTime.UtcNow - _startTime;
        if (_deltaTime >= TimeSpan.FromMinutes(_osuFileTTL / 2))
        {
            _deltaTime = TimeSpan.Zero;
            _startTime = DateTime.UtcNow;
        }
        return _deltaTime == TimeSpan.Zero;
    }
    
    public void CheckCache()
    {
        if (!ShouldCleanUp()) return;
        
        _logger.Log(LogLevel.Information, "Checking if any files in cache folder exceeded their TTL...");
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