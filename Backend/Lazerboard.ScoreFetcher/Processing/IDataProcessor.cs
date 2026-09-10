using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;

namespace Lazerboard.ScoreFetcher.Processing;

public interface IDataProcessor
{
    Task ProcessBeatmapsetsAsync(IList<APIBeatmapset> beatmapsets, ScanEventType scanEventType, CancellationToken ct);
    Task ProcessBeatmapsAsync(IList<APIBeatmap> beatmaps, CancellationToken ct);
    Task<List<Beatmap>> GetExistingBeatmapsAsync(IList<int> ids, CancellationToken ct);
    Task<List<Beatmapset>> GetExistingBeatmapsetsAsync(IList<int> ids, CancellationToken ct);
    Task<List<User>> GetExistingUsersAsync(IList<int> ids, CancellationToken ct);
    Task ProcessCountriesAsync(IList<APICountry> countries, CancellationToken ct);
    Task ProcessUsersAsync(IList<APIUser> users, CancellationToken ct);
    Task ProcessRemovedUsersAsync(IList<User> users, CancellationToken ct);
    Task<int> ProcessScoresAsync(IList<APIScore> scores, ScoreSource source, CancellationToken ct);
    Task<List<int>> GetBeatmapIdsWithScoresAsync(IList<int> beatmapIds, CancellationToken ct);
    Task<Score?> GetMaxFirehoseScoreAsync(CancellationToken cancellationToken);
    Task<int> GetSecondHighestBeatmapsetIdAsync(CancellationToken cancellationToken);
}