using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;

namespace Lazerboard.ScoreFetcher.Processing;

public interface IDataProcessor
{
    Task ProcessBeatmapsetsAsync(IEnumerable<APIBeatmapset> beatmapsets, CancellationToken ct);
    Task ProcessBeatmapsAsync(IEnumerable<APIBeatmap> beatmaps, CancellationToken ct);
    Task<List<Beatmap>> GetExistingBeatmapsAsync(IEnumerable<int> ids, CancellationToken ct);
    Task<List<Beatmapset>> GetExistingBeatmapsetsAsync(IEnumerable<int> ids, CancellationToken ct);
    Task<List<User>> GetExistingUsersAsync(IEnumerable<int> ids, CancellationToken ct);
    Task ProcessCountriesAsync(IEnumerable<APICountry> countries, CancellationToken ct);
    Task ProcessUsersAsync(IEnumerable<APIUser> users, CancellationToken ct);
    Task ProcessRemovedUsersAsync(IEnumerable<User> users, CancellationToken ct);
    Task ProcessScoresAsync(IEnumerable<APIScore> scores, ScoreSource source, CancellationToken ct);
    Task<List<int>> GetBeatmapIdsWithScoresAsync(IList<int> beatmapIds, CancellationToken ct);
    Task<Score?> GetMaxFirehoseScoreAsync(CancellationToken cancellationToken);
    Task<int> GetSecondHighestBeatmapsetIdAsync(CancellationToken cancellationToken);
}