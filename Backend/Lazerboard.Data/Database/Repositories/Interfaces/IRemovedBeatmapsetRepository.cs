using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IRemovedBeatmapsetRepository : IRepository<RemovedBeatmapset, int>
{
    // Only exists to have things the same as other repositories for now.
}