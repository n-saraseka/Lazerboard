using OsuScoreStats.DbService.Entities;
using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.Processing;

public interface IDataProcessor
{
    Task ProcessBeatmapsetsAsync(IList<APIBeatmapset> beatmapsets, CancellationToken ct);
    Task ProcessBeatmapsAsync(IList<APIBeatmap> beatmaps, CancellationToken ct);
    Task<List<Beatmap>> GetExistingBeatmapsAsync(IList<int> ids, CancellationToken ct);
    Task<List<Beatmapset>> GetExistingBeatmapsetsAsync(IList<int> ids, CancellationToken ct);
    Task<List<User>> GetExistingUsersAsync(IList<int> ids, CancellationToken ct);
    Task ProcessCountriesAsync(IList<APICountry> countries, CancellationToken ct);
    Task ProcessUsersAsync(IList<APIUser> users, CancellationToken ct);
    Task ProcessRemovedUsersAsync(IList<User> users, CancellationToken ct);
    Task ProcessScoresAsync(IList<APIScore> scores, CancellationToken ct);
}