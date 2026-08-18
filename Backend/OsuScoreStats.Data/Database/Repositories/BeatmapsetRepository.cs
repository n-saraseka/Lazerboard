using OsuScoreStats.Data.Database.Entities;
using OsuScoreStats.Data.Database.Repositories.Interfaces;

namespace OsuScoreStats.Data.Database.Repositories;

public class BeatmapsetRepository(ScoreDataContext db) : BaseRepository<Beatmapset, int>(db), IBeatmapsetRepository
{ 
    // Only exists to keep things the same as other repositories for now.
}
