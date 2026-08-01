using OsuScoreStats.Calculations;

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
        var scoreProcessor = scope.ServiceProvider.GetRequiredService<IScoreProcessor>();
        var cacheStore = scope.ServiceProvider.GetRequiredService<ICacheStore>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        _apiInterval = double.Parse(config["OsuApiInterval"]);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            cacheStore.CheckCache();
            
            var scoresResponse = await apiFetcher.GetScoresAsync(_cursor, stoppingToken);
            _cursor = scoresResponse.Cursor;
            var scores = scoresResponse.Scores;
            await Task.Delay(TimeSpan.FromSeconds(_apiInterval), stoppingToken);
            
            var checkResults = await scoreProcessor.CheckIfSignificantBulkAsync(scores, stoppingToken);
            var significantScores = scores.Where(s => checkResults[s.Id]).ToList();
            
            // Calculate PP for scores that don't have it.
            var scoresWithoutPp = significantScores.Where(s => s.PP == null).ToList();
            foreach (var score in scoresWithoutPp)
            {
                await scoreProcessor.CalculateScoreAsync(score, stoppingToken);
            }

            var users = significantScores.Select(s => s.User).Distinct();
            var countries = users.Select(u => u.Country).Distinct();
            await dataProcessor.ProcessCountriesAsync(countries, stoppingToken);
            await dataProcessor.ProcessUsersAsync(users, stoppingToken);
            
            var beatmapIds = significantScores.Select(s => s.BeatmapId).Distinct();
            var existingBeatmaps = await dataProcessor.GetExistingBeatmapsAsync(beatmapIds, stoppingToken);
            var newBeatmapIds = beatmapIds.Where(id => !existingBeatmaps.Select(b => b.Id).Contains(id)).ToList();
            var beatmaps = await apiFetcher.GetBeatmapsAsync(newBeatmapIds, stoppingToken);
            var beatmapsets = beatmaps.Select(b => b.Beatmapset).Distinct();
            await dataProcessor.ProcessBeatmapsetsAsync(beatmapsets, stoppingToken);
            await dataProcessor.ProcessBeatmapsAsync(beatmaps, stoppingToken);
            
            await dataProcessor.ProcessScoresAsync(significantScores, stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(RepeatAfterSeconds), stoppingToken);
        }
    }
}