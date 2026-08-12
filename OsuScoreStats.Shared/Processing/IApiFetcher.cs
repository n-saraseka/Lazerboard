using OsuScoreStats.Shared.OsuApi.Enums;
using OsuScoreStats.Shared.OsuApi.OsuApiEntities;

namespace OsuScoreStats.Shared.Processing;

public interface IApiFetcher
{
    public Task<BeatmapsetsResponse> SearchBeatmapsetsAsync(string? cursor, CancellationToken ct = default);
    public Task<BeatmapScores> GetBeatmapScoresAsync(APIBeatmap beatmap, Mode? mode, int legacyOnly = 0, CancellationToken ct = default);
    public Task<ScoresResponse> GetScoresAsync(string? cursor, CancellationToken ct = default);
    public Task<List<APIUser>> GetUsersAsync(IList<int> userIds, CancellationToken ct = default);
    public Task<List<APIBeatmap>> GetBeatmapsAsync(IList<int> beatmapIds, CancellationToken ct = default);
}