using Microsoft.EntityFrameworkCore;
using OsuScoreStats.Data.Database.Entities;
using OsuScoreStats.Data.Database.Repositories.Interfaces;

namespace OsuScoreStats.Data.Database.Repositories;

public class BeatmapRepository(ScoreDataContext db) : BaseRepository<Beatmap, int>(db), IBeatmapRepository
{
    public Task<List<Beatmap>> GetByBeatmapsetIdAsync(int beatmapsetId, CancellationToken ct) =>
        Set
            .Where(b => b.BeatmapsetId == beatmapsetId)
            .OrderBy(b => b.Mode)
            .ThenBy(b => b.Difficulty)
            .Include(b => b.Beatmapset)
            .ThenInclude(bs => bs.User)
            .ToListAsync(ct);
}
