using OsuScoreStats.OsuApi.Enums;
using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.ScoreFetcher;

public interface IApiFetcher
{
    public Task<BeatmapsetsResponse> SearchBeatmapsetsAsync(string? cursor, CancellationToken ct = default);
    public Task<BeatmapScores> GetBeatmapScoresAsync(APIBeatmap beatmap, Mode? mode, int legacyOnly = 0, CancellationToken ct = default);
    public Task<ScoresResponse> GetScoresAsync(string? cursor, CancellationToken ct = default);
    public Task<List<APIUser>> GetUsersAsync(IEnumerable<int> userIds, CancellationToken ct = default);
    public Task<List<APIBeatmap>> GetBeatmapsAsync(IEnumerable<int> beatmapIds, CancellationToken ct = default);
}