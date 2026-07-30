using osu.Game.IO;
using osu.Game.Beatmaps;
using OsuScoreStats.OsuApi;

namespace OsuScoreStats.Calculations;

public class CacheStore : ICacheStore
{
    private IConfiguration _config;
    private OsuApiService _osuApiService;
    private string _cachePath;
    private int _osuFileTTL;
    private const int DefaultTTL = 10;
    private TimeSpan _deltaTime;
    private DateTime _startTime;

    public CacheStore(IConfiguration config, OsuApiService osuApiService)
    {
        _config = config;
        _osuApiService = osuApiService;
        _cachePath = _config["CacheFolder"];
        _osuFileTTL = int.TryParse(_config["osuFileTTL"], out var osuFileTTL) ? osuFileTTL : DefaultTTL;
        _deltaTime = TimeSpan.Zero;
        _startTime = DateTime.UtcNow;
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
        
        Console.WriteLine("Checking if any files in cache folder exceeded their TTL...");
        var deletedCount = 0;
        var files = Directory.EnumerateFiles(_cachePath);
        foreach (var file in files)
            if (DateTime.UtcNow - File.GetCreationTimeUtc(file) >= TimeSpan.FromMinutes(_osuFileTTL))
            {
                File.Delete(file);
                deletedCount++;
            }
        Console.WriteLine($"Deleted {deletedCount} files from beatmap cache");
    }
    
    public async Task<Beatmap> GetBeatmapFileAsync(int beatmapId, CancellationToken ct)
    {
        var mapPath = $"{_cachePath}/{beatmapId}.osu";
        if (!File.Exists(mapPath))
        {
            await _osuApiService.DownloadBeatmapAsync(beatmapId, ct);
            await Task.Delay(TimeSpan.FromSeconds(1), ct);
        }
        
        await using var stream = File.OpenRead(mapPath);
        using var reader = new LineBufferedReader(stream);
        return osu.Game.Beatmaps.Formats.Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
    }
}