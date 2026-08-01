using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.DbService.Entities;
namespace OsuScoreStats.DbService.Repositories;

public class BeatmapRepository(ScoreDataContext db) : BaseRepository<Beatmap, int>(db), IBeatmapRepository
{
    public Task<List<Beatmap>> GetByBeatmapsetIdAsync(int beatmapsetId, CancellationToken ct) =>
        Set
            .Where(b => b.BeatmapsetId == beatmapsetId)
            .OrderBy(b => b.Difficulty)
            .Include(b => b.Beatmapset)
            .ToListAsync(ct);
}
