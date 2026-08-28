using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Data.Database.Repositories;

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
    
    public Task<List<Beatmap>> GetBulkWithBeatmapsetsAsync(IList<int> ids, CancellationToken ct) =>
        Set.
            Where(b => ids.Contains(b.Id)).
            Include(b => b.Beatmapset)
            .ToListAsync(ct);
}
