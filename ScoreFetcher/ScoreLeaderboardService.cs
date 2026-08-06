using OsuScoreStats.Calculations;
using OsuScoreStats.OsuApi.Enums;
using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.ScoreFetcher;

public class ScoreLeaderboardService : BackgroundService
{
    private IServiceProvider _serviceProvider;
    private ILogger<ScoreLeaderboardService> _logger;
    private string? _cursor;
    private double _apiInterval;

    public ScoreLeaderboardService(IServiceProvider serviceProvider, ILogger<ScoreLeaderboardService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        _apiInterval = double.Parse(config["OsuApiInterval"]);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var apiFetcher = scope.ServiceProvider.GetRequiredService<IApiFetcher>();
            var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
            var cacheStore = scope.ServiceProvider.GetRequiredService<ICacheStore>();
            
            var utils = scope.ServiceProvider.GetRequiredService<ScoreFetchingUtils>();
            
            cacheStore.CheckCache();
            
            _logger.Log(LogLevel.Information, "Searching beatmapsets. Cursor: {cursor}", _cursor);
            var beatmapsetsResponse = await apiFetcher.SearchBeatmapsetsAsync(_cursor, stoppingToken);
            _cursor = beatmapsetsResponse.Cursor;
            await Task.Delay(TimeSpan.FromSeconds(_apiInterval), stoppingToken);
            
            var beatmapsets = beatmapsetsResponse.Beatmapsets;
            await utils.SaveAllBeatmapsetDataAsync(beatmapsets, stoppingToken);
            
            var beatmaps = new List<APIBeatmap>();
            foreach (var beatmapset in beatmapsets)
                beatmaps.AddRange(beatmapset.Beatmaps);
            await dataProcessor.ProcessBeatmapsAsync(beatmaps, stoppingToken);

            var scores = new List<APIScore>();
            foreach (var beatmap in beatmaps)
            {
                var beatmapScores = new List<BeatmapScores>();
                
                foreach (var val in Enum.GetValues<Mode>())
                {
                    if (beatmap.Mode != Mode.Osu && beatmap.Mode != val) continue;
                    _logger.Log(LogLevel.Information, "Getting leaderboard scores. BeatmapID: {id}; Mode: {mode}", beatmap.Id, val);
                    beatmapScores.Add(await apiFetcher.GetBeatmapScoresAsync(beatmap, val, 0, stoppingToken));
                    await Task.Delay(TimeSpan.FromSeconds(_apiInterval), stoppingToken);
                }
                
                scores.AddRange(beatmapScores.SelectMany(bs => bs.Scores));
                await Task.Delay(TimeSpan.FromSeconds(_apiInterval), stoppingToken);
            }
            
            var significantScores = await utils.GetSignificantScoresAsync(scores, stoppingToken);
            _logger.Log(LogLevel.Information, "SignificantScoreCount: {count}", significantScores.Count);
            await utils.SaveUserDataFromScoresAsync(significantScores,  stoppingToken);
            await dataProcessor.ProcessScoresAsync(significantScores, stoppingToken);
        }
    }
}