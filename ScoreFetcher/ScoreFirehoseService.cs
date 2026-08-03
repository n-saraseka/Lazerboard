using OsuScoreStats.Calculations;
using OsuScoreStats.DbService.Entities;

namespace OsuScoreStats.ScoreFetcher;

public class ScoreFirehoseService(IServiceProvider serviceProvider) : BackgroundService
{
    private string? _cursor;
    private double _apiInterval;
    private const int RepeatAfterSeconds = 10;

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
            
            var scoresResponse = await apiFetcher.GetScoresAsync(_cursor, stoppingToken);
            _cursor = scoresResponse.Cursor;
            var scores = scoresResponse.Scores;
            await Task.Delay(TimeSpan.FromSeconds(_apiInterval), stoppingToken);
            
            var significantScores = await utils.GetSignificantScoresAsync(scores, stoppingToken);
            await utils.SaveUserDataFromScoresAsync(significantScores,  stoppingToken);
            
            var beatmapIds = significantScores.Select(s => s.BeatmapId).Distinct();
            var existingBeatmaps = await dataProcessor.GetExistingBeatmapsAsync(beatmapIds, stoppingToken);
            var newBeatmapIds = beatmapIds.Where(id => !existingBeatmaps.Select(b => b.Id).Contains(id)).ToList();
            var beatmaps = await apiFetcher.GetBeatmapsAsync(newBeatmapIds, stoppingToken);
            
            var beatmapsets = beatmaps.Select(b => b.Beatmapset).Distinct().ToList();
            await utils.SaveAllBeatmapsetDataAsync(beatmapsets, stoppingToken);
            await dataProcessor.ProcessBeatmapsAsync(beatmaps, stoppingToken);
            
            await dataProcessor.ProcessScoresAsync(significantScores, stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(RepeatAfterSeconds), stoppingToken);
        }
    }
}