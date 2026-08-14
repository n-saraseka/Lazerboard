using OsuScoreStats.DbService.Entities;

namespace OsuScoreStats.DbService.Repositories.Interfaces;

public interface IBeatmapRepository : IRepository<Beatmap, int>
{
    Task<List<Beatmap>> GetByBeatmapsetIdAsync(int beatmapsetId, CancellationToken ct);
}