using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Data.Database.Repositories;

public class BeatmapRepository(ScoreDataContext db) : BaseRepository<Beatmap, int>(db), IBeatmapRepository
{
    public Task<List<Beatmap>> GetByBeatmapsetIdAsync(int beatmapsetId, CancellationToken ct) =>
        Set
            .AsNoTracking()
            .Where(b => b.BeatmapsetId == beatmapsetId)
            .Include(b => b.Beatmapset)
            .ThenInclude(bs => bs.User)
            .ToListAsync(ct);
    
    public Task<List<Beatmap>> GetBulkWithBeatmapsetsAsync(IList<int> ids, CancellationToken ct) =>
        Set
            .AsNoTracking()
            .Where(b => ids.Contains(b.Id))
            .Include(b => b.Beatmapset)
            .ToListAsync(ct);
    
    public Task<Beatmap?> GetWithBeatmapsetDataAsync(int id, CancellationToken ct = default) =>
        Set
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Include(b => b.Beatmapset)
            .ThenInclude(bs => bs.User)
                    .FirstOrDefaultAsync(ct);

    /// <summary>
    /// Get <see cref="Beatmap"/>s that have <see cref="Score"/>s
    /// </summary>
    /// <param name="ids">A list of <see cref="Beatmap.Id"/>s</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>The</returns>
    public Task<List<Beatmap>> GetBeatmapsWithScoresAsync(IList<int> ids, CancellationToken ct = default) =>
        Set
            .Where(b => ids.Contains(b.Id))
            .Include(b => b.Scores)
            .AsSplitQuery()
            .Where(b => b.Scores.Count > 0)
            .ToListAsync(ct);
}
