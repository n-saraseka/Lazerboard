using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.OsuEntities.Enums;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IScoreRepository : IRepository<Score, ulong>
{
    Task<List<Score>> GetByBeatmapIdAsync(int beatmapId, CancellationToken cancellationToke = default);
    Task<List<Score>> GetByBeatmapIdsAsync(IEnumerable<int> beatmapIds, CancellationToken cancellationToken = default);
    Task<List<Score>> GetByBeatmapIdWithUserDataAsync(int beatmapId, Mode mode, int page, CancellationToken cancellationToken = default);
    Task<int> GetBeatmapScoreCount(int beatmapId, Mode mode, CancellationToken cancellationToken = default);
    Task<int> GetMaxBeatmapsetIdAsync(CancellationToken cancellationToken = default);
    Task<ulong> GetMaxScoreIdAsync(CancellationToken cancellationToken);
}