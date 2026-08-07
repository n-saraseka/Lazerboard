using OsuScoreStats.DbService.Entities;
using OsuScoreStats.OsuApi.Enums;

namespace OsuScoreStats.DbService.Repositories.Interfaces;

public interface IScoreRepository : IRepository<Score, ulong>
{
    Task<List<Score>> GetByBeatmapIdAsync(int beatmapId, CancellationToken cancellationToken);
    Task<List<Score>> GetByBeatmapIdsAsync(IEnumerable<int> beatmapIds, CancellationToken cancellationToken);
    IQueryable<Score> GetAllWithBeatmapAndUserData();
    IQueryable<Score> GetAllWithUserData();
    Task<List<Score>> GetByBeatmapIdWithUserDataAsync(int beatmapId, Mode mode, CancellationToken cancellationToken);
}