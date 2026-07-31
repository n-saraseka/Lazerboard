using OsuScoreStats.DbService.Entities;

namespace OsuScoreStats.DbService.Repositories.Interfaces;

public interface IScoreRepository : IRepository<Score, ulong>
{
    Task<List<Score>> GetByBeatmapIdAsync(int beatmapId, CancellationToken cancellationToken);
    Task<List<IGrouping<int, Score>>> GetByBeatmapIdsAsync(IEnumerable<int> beatmapIds, CancellationToken cancellationToken);
    IQueryable<Score> GetAllWithBeatmapAndUserData();
}