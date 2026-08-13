using OsuScoreStats.DbService.Entities;

namespace OsuScoreStats.DbService.Repositories.Interfaces;

public interface IBeatmapsetRepository : IRepository<Beatmapset, int>
{
    // Only exists to keep things the same as other repositories for now.
}