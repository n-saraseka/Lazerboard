using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories.Interfaces;

namespace OsuScoreStats.DbService.Repositories;

public class BeatmapsetRepository(ScoreDataContext db) : BaseRepository<Beatmapset, int>(db), IBeatmapsetRepository
{ 
    // Only exists to keep things the same as other repositories for now.
}
