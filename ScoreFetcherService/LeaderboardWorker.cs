using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.ScoreFetcherService;

public class LeaderboardWorker(IScoreFetcher scoreFetcher) : BackgroundService
{
    private string? _cursor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var beatmapsetsResponse = await scoreFetcher.ProcessBeatmapsetSearchAsync(_cursor, stoppingToken);
            _cursor = beatmapsetsResponse.Cursor;
            var beatmapsets = beatmapsetsResponse.Beatmapsets;
            var beatmaps = new List<APIBeatmap>();
            foreach (var beatmapset in beatmapsets)
                beatmaps.AddRange(beatmapset.Beatmaps);
            
            foreach (var beatmap in beatmaps)
            {
                await scoreFetcher.GetBeatmapScoresAsync(beatmap, beatmap.Mode, 0, stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}