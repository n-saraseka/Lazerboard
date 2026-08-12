using OsuScoreStats.Shared.DbService.Entities;

namespace OsuScoreStats.Shared.DbService.Repositories.Interfaces;

public interface IBeatmapRepository : IRepository<Beatmap, int>
{
    Task<List<Beatmap>> GetByBeatmapsetIdAsync(int beatmapsetId, CancellationToken ct);
}