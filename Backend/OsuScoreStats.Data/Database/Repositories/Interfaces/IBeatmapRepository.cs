using OsuScoreStats.Data.Database.Entities;

namespace OsuScoreStats.Data.Database.Repositories.Interfaces;

public interface IBeatmapRepository : IRepository<Beatmap, int>
{
    Task<List<Beatmap>> GetByBeatmapsetIdAsync(int beatmapsetId, CancellationToken ct);
}