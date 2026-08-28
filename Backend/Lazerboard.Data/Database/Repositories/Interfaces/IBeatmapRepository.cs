using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IBeatmapRepository : IRepository<Beatmap, int>
{
    Task<List<Beatmap>> GetByBeatmapsetIdAsync(int beatmapsetId, CancellationToken ct);
    Task<List<Beatmap>> GetBulkWithBeatmapsetsAsync(IList<int> ids, CancellationToken ct);
}