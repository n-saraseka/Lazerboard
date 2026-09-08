using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;

namespace Lazerboard.Data.ApiFetchers;

public interface IOsuApiFetcher
{
    Task<BeatmapsetsResponse> SearchBeatmapsetsAsync(string? cursor, CancellationToken ct = default);
    Task<BeatmapScores> GetBeatmapScoresAsync(int beatmapId, Mode? mode, int legacyOnly = 0, CancellationToken ct = default);
    Task<ScoresResponse> GetScoresAsync(string? cursor, CancellationToken ct = default);
    Task<List<APIUser>> GetUsersAsync(IList<int> userIds, CancellationToken ct = default);
    Task<List<APIBeatmap>> GetBeatmapsAsync(IList<int> beatmapIds, CancellationToken ct = default);
    Task<APIBeatmapset> GetBeatmapsetAsync(int beatmapsetId, CancellationToken ct = default);
    Task<Stream> DownloadBeatmapAsync(int beatmapId, CancellationToken ct = default);
}