using Lazerboard.Data.Database.Entities;
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
    
    public Task<int> GetMaxBeatmapIdAsync(CancellationToken cancellationToken) =>
        Set.MaxAsync(s => s.BeatmapId, cancellationToken);
}
