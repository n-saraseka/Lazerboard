using OsuScoreStats.Calculations;
using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.ScoreFetcher;

public class ScoreLeaderboardService(IServiceProvider serviceProvider) : BackgroundService
{
    private string? _cursor;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IApiFetcher>();
        var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
        var scoreProcessor = scope.ServiceProvider.GetRequiredService<IScoreProcessor>();
        var cacheStore = scope.ServiceProvider.GetRequiredService<ICacheStore>();
            
        while (!stoppingToken.IsCancellationRequested)
        {
            cacheStore.CheckCache();
            
            var beatmapsetsResponse = await apiFetcher.SearchBeatmapsetsAsync(_cursor, stoppingToken);
            _cursor = beatmapsetsResponse.Cursor;
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            
            var beatmapsets = beatmapsetsResponse.Beatmapsets;
            await dataProcessor.ProcessBeatmapsetsAsync(beatmapsets, stoppingToken);
            
            var beatmaps = new List<APIBeatmap>();
            foreach (var beatmapset in beatmapsets)
                beatmaps.AddRange(beatmapset.Beatmaps);
            await dataProcessor.ProcessBeatmapsAsync(beatmaps, stoppingToken);

            var scores = new List<APIScore>();
            foreach (var beatmap in beatmaps)
            {
                var beatmapScores = await apiFetcher.GetBeatmapScoresAsync(beatmap, beatmap.Mode, 0, stoppingToken);
                scores.AddRange(beatmapScores.Scores);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            
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
            
            await dataProcessor.ProcessScoresAsync(significantScores, stoppingToken);
        }
    }
}