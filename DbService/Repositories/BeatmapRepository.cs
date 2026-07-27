using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.DbService.Entities;
namespace OsuScoreStats.DbService.Repositories;

public class BeatmapRepository(ScoreDataContext db) : BaseRepository<Beatmap, int>(db), IBeatmapRepository
    { 
        // Only exists to keep things the same as other repositories for now.
}
