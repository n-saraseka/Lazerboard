using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Data.Database.Repositories;

public class ScoreRepository(ScoreDataContext db) : BaseRepository<Score, ulong>(db), IScoreRepository
{
    public Task<List<Score>> GetByBeatmapIdAsync(int beatmapId, CancellationToken cancellationToken) =>
        Set.Where(s => s.BeatmapId == beatmapId).ToListAsync(cancellationToken);
    
    public Task<List<Score>> GetByBeatmapIdWithUserDataAsync(int beatmapId, Mode mode, int page, CancellationToken cancellationToken) =>
        Set
            .Where(s => s.BeatmapId == beatmapId && s.Mode == mode)
            .OrderBy(s => s.Rank)
            .Skip(100 * (page - 1))
            .Take(100)
            .Include(s => s.User)
            .ThenInclude(u => u.Country)
            .ToListAsync(cancellationToken);
    
    public Task<int> GetBeatmapScoreCount(int beatmapId, Mode mode, CancellationToken cancellationToken) => Set
        .Where(s => s.BeatmapId == beatmapId && s.Mode == mode).CountAsync(cancellationToken);

    public Task<List<Score>> GetByBeatmapIdsAsync(IEnumerable<int> beatmapIds, CancellationToken cancellationToken) =>
        Set.Where(s => beatmapIds.Contains(s.BeatmapId)).ToListAsync(cancellationToken);

    // We have to do this because the generated LINQ by EF Core is literally 15 times more inefficient. (0.1s execution time vs 1.5s on a test DB)
    public Task<int> GetSecondHighestBeatmapsetIdAsync(CancellationToken cancellationToken) =>
        GetDbContext()
            .Database
            .SqlQueryRaw<int>("SELECT DISTINCT b.beatmapset_id AS \"Value\"\nFROM scores s INNER JOIN beatmaps b ON s.beatmap_id = b.id\nORDER BY b.beatmapset_id DESC\nOFFSET 1\nLIMIT 1")
            .FirstOrDefaultAsync(cancellationToken);
    
    public Task<int> GetMaxBeatmapsetIdAsync(CancellationToken cancellationToken) =>
        GetDbContext()
            .Database
            .SqlQueryRaw<int>("SELECT MAX(b.beatmapset_id) AS \"Value\"\nFROM scores s INNER JOIN beatmaps b ON s.beatmap_id = b.id")
            .FirstOrDefaultAsync(cancellationToken);

    public Task<ulong> GetMaxFirehoseScoreIdAsync(CancellationToken cancellationToken) =>
        Set
            .Where(s => s.ScoreSource == ScoreSource.ScoreFetcher)
            .MaxAsync(s => s.Id, cancellationToken);
}
