using OsuScoreStats.Calculations;
using OsuScoreStats.DbService.Entities;
using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.ScoreFetcher;

public class ScoreLeaderboardService(IServiceProvider serviceProvider) : BackgroundService
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
                var beatmapScores = await apiFetcher.GetBeatmapScoresAsync(beatmap, beatmap.Mode, 0, stoppingToken);
                scores.AddRange(beatmapScores.Scores);
                await Task.Delay(TimeSpan.FromSeconds(_apiInterval), stoppingToken);
            }
            
            var significantScores = await utils.GetSignificantScoresAsync(scores, stoppingToken);
            await utils.SaveUserDataFromScoresAsync(significantScores,  stoppingToken);
            await dataProcessor.ProcessScoresAsync(significantScores, stoppingToken);
        }
    }
}