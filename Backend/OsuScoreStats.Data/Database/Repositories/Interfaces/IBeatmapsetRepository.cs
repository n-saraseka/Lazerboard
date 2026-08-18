using OsuScoreStats.Data.Database.Entities;

namespace OsuScoreStats.Data.Database.Repositories.Interfaces;

public interface IBeatmapsetRepository : IRepository<Beatmapset, int>
{
    // Only exists to keep things the same as other repositories for now.
}