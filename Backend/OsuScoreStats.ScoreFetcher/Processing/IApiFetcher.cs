using OsuScoreStats.Data.OsuEntities.Enums;
using OsuScoreStats.Data.OsuEntities.OsuApiEntities;

namespace OsuScoreStats.ScoreFetcher.Processing;

public interface IApiFetcher
{
    public Task<BeatmapsetsResponse> SearchBeatmapsetsAsync(string? cursor, CancellationToken ct = default);
    public Task<BeatmapScores> GetBeatmapScoresAsync(APIBeatmap beatmap, Mode? mode, int legacyOnly = 0, CancellationToken ct = default);
    public Task<ScoresResponse> GetScoresAsync(string? cursor, CancellationToken ct = default);
    public Task<List<APIUser>> GetUsersAsync(IEnumerable<int> userIds, CancellationToken ct = default);
    public Task<List<APIBeatmap>> GetBeatmapsAsync(IEnumerable<int> beatmapIds, CancellationToken ct = default);
}