using OsuScoreStats.Calculations;
using OsuScoreStats.OsuApi.Enums;
using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.ScoreFetcher;

public class ScoreLeaderboardService(IServiceProvider serviceProvider, ILogger<ScoreLeaderboardService> logger) : BackgroundService
{
    private string? _cursor;
    private double _apiInterval;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IApiFetcher>();
        var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
        var cacheStore = scope.ServiceProvider.GetRequiredService<ICacheStore>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var utils = scope.ServiceProvider.GetRequiredService<ScoreFetchingUtils>();
        
        _apiInterval = double.Parse(config["OsuApiInterval"]);
            
        while (!stoppingToken.IsCancellationRequested)
        {
            cacheStore.CheckCache();
            
            logger.Log(LogLevel.Information, "Searching beatmapsets; Cursor: {cursor}", _cursor);
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
                    logger.Log(LogLevel.Information, "Getting leaderboard scores; BeatmapID: {id}, {mode}", beatmap.Id, beatmap.Mode);
                    beatmapScores.Add(await apiFetcher.GetBeatmapScoresAsync(beatmap, val, 0, stoppingToken));
                    await Task.Delay(TimeSpan.FromSeconds(_apiInterval), stoppingToken);
                }
                
                scores.AddRange(beatmapScores.SelectMany(bs => bs.Scores));
                await Task.Delay(TimeSpan.FromSeconds(_apiInterval), stoppingToken);
            }
            
            var significantScores = await utils.GetSignificantScoresAsync(scores, stoppingToken);
            logger.Log(LogLevel.Information, "SignificantScoreCount: {count}", significantScores.Count);
            await utils.SaveUserDataFromScoresAsync(significantScores,  stoppingToken);
            await dataProcessor.ProcessScoresAsync(significantScores, stoppingToken);
        }
    }
}