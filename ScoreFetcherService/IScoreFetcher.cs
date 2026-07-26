using OsuScoreStats.OsuApi.Enums;
using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.ScoreFetcherService;

public interface IScoreFetcher
{
    public Task<BeatmapsetsResponse> ProcessBeatmapsetSearchAsync(string? cursor, CancellationToken ct = default);
    public Task<BeatmapScores> GetBeatmapScoresAsync(APIBeatmap beatmap, Mode? mode, int legacyOnly = 0, CancellationToken ct = default);
    public Task<ScoresResponse> GetScoresAsync(string? cursor, CancellationToken ct = default);
    public Task ProcessUnrankedScoresAsync(IEnumerable<Score> scores, CancellationToken ct = default);
    public Task ProcessRankedScoresAsync(IEnumerable<Score> scores, CancellationToken ct = default);
    public Task GetUsersFromApiAsync(IEnumerable<int> userIds, CancellationToken ct = default);
    public Task GetBeatmapsFromApiAsync(IEnumerable<int> beatmapIds, CancellationToken ct = default);
}