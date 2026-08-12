using OsuScoreStats.Shared.DbService.Entities;

namespace OsuScoreStats.Shared.DbService.Repositories.Interfaces;

public interface IBeatmapsetRepository : IRepository<Beatmapset, int>
{
    // Only exists to keep things the same as other repositories for now.
}