using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;

namespace Lazerboard.Data.Database.Repositories;

public class BeatmapsetRepository(ScoreDataContext db) : BaseRepository<Beatmapset, int>(db), IBeatmapsetRepository
{ 
    // Only exists to keep things the same as other repositories for now.
}
