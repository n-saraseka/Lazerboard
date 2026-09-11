using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;

namespace Lazerboard.Data.Database.Repositories;

public class RemovedBeatmapsetRepository(ScoreDataContext db) : BaseRepository<RemovedBeatmapset, int>(db), IRemovedBeatmapsetRepository
{
    // Only exists to have things the same as other repositories for now.
}