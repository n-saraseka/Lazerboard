using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.OsuEntities.Enums;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IBeatmapRepository : IRepository<Beatmap, int>
{
    Task<List<Beatmap>> GetByBeatmapsetIdAsync(int beatmapsetId, CancellationToken ct = default);
    Task<List<Beatmap>> GetBulkWithBeatmapsetsAsync(IList<int> ids, CancellationToken ct = default);
    Task<Beatmap?> GetWithBeatmapsetDataAsync(int id, CancellationToken ct = default);
    Task<List<int>> GetBeatmapsIdsWithScoresAsync(IList<int> ids, CancellationToken ct = default);
    Task<List<int>> GetBeatmapsIdsFromProcessedMapsetsAync(IList<int> ids, CancellationToken ct = default);
    Task<Dictionary<int, Mode>> GetModeDataAsync(IList<int> ids, CancellationToken ct = default);
}