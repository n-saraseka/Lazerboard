namespace OsuScoreStats.ScoreFetcher;

public class ScoreFirehoseService(IApiFetcher apiFetcher,
    IDataProcessor dataProcessor,
    IScoreProcessor scoreProcessor) : BackgroundService
{
    private string? _cursor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var scoresResponse = await apiFetcher.GetScoresAsync(_cursor, stoppingToken);
            _cursor = scoresResponse.Cursor;
            var scores = scoresResponse.Scores;
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            
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
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}