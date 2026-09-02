using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IBeatmapRepository : IRepository<Beatmap, int>
{
    Task<List<Beatmap>> GetByBeatmapsetIdAsync(int beatmapsetId, CancellationToken ct = default);
    Task<List<Beatmap>> GetBulkWithBeatmapsetsAsync(IList<int> ids, CancellationToken ct = default);
    Task<Beatmap?> GetWithBeatmapsetDataAsync(int id, CancellationToken ct = default);
}