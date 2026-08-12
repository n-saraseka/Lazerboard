using OsuScoreStats.Shared.DbService.Entities;
using OsuScoreStats.Shared.DbService.Repositories.Interfaces;

namespace OsuScoreStats.Shared.DbService.Repositories;

public class BeatmapsetRepository(ScoreDataContext db) : BaseRepository<Beatmapset, int>(db), IBeatmapsetRepository
{ 
    // Only exists to keep things the same as other repositories for now.
}
