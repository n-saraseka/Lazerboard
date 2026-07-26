using OsuScoreStats.DbService.Entities;

namespace OsuScoreStats.DbService.Repositories.Interfaces;

public interface IBeatmapRepository : IRepository<Beatmap, int>
{
    // Only exists to keep things the same as other repositories for now.
}