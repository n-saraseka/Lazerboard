using OsuScoreStats.DbService.Entities;
using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.ScoreFetcher;

public interface IDataProcessor
{
    Task ProcessBeatmapsetsAsync(IEnumerable<APIBeatmapset> beatmapsets, CancellationToken ct);
    Task ProcessBeatmapsAsync(IEnumerable<APIBeatmap> beatmaps, CancellationToken ct);
    Task<List<Beatmap>> GetExistingBeatmapsAsync(IEnumerable<int> ids, CancellationToken ct);
    Task<List<Beatmapset>> GetExistingBeatmapsetsAsync(IEnumerable<int> ids, CancellationToken ct);
    Task ProcessCountriesAsync(IEnumerable<APICountry> countries, CancellationToken ct);
    Task ProcessUsersAsync(IEnumerable<APIUser> users, CancellationToken ct);
    Task ProcessRemovedUsersAsync(IEnumerable<User> users, CancellationToken ct);
    Task ProcessScoresAsync(IEnumerable<APIScore> scores, CancellationToken ct);
}