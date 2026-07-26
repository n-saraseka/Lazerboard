namespace OsuScoreStats.ScoreFetcherService;

public class ScoreWorker(IScoreFetcher scoreFetcher) : BackgroundService
{
    private string? _cursor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var scoresResponse = await scoreFetcher.GetScoresAsync(_cursor, stoppingToken);
            _cursor = scoresResponse.Cursor;
            var scores = scoresResponse.Scores;
            // process beatmap and user data first
            var userIds = scores.Select(s => s.UserId).Distinct();
            var beatmapIds = scores.Select(s => s.BeatmapId).Distinct();
            var usersTask = scoreFetcher.GetUsersFromApiAsync(userIds, stoppingToken);
            var beatmapsTask = scoreFetcher.GetBeatmapsFromApiAsync(beatmapIds, stoppingToken);
            
            await Task.WhenAll(usersTask, beatmapsTask);
            
            // then can process score data
            Task rankedScoresTask = scoreFetcher.ProcessRankedScoresAsync(scores.Where(s => s.PP != null), stoppingToken);
            Task unrankedScoresTask = scoreFetcher.ProcessUnrankedScoresAsync(scores.Where(s => s.PP == null), stoppingToken);
            await Task.WhenAll(rankedScoresTask, unrankedScoresTask);
        }
    }
}