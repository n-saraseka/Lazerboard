using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories.Interfaces;

namespace OsuScoreStats.DbService.Repositories;

public class ScoreRepository(ScoreDataContext db) : BaseRepository<Score, ulong>(db), IScoreRepository
{
    public Task<List<Score>> GetByBeatmapIdAsync(int beatmapId, CancellationToken cancellationToken) =>
        Set.Where(s => s.BeatmapId == beatmapId).ToListAsync(cancellationToken);
    
    public Task<List<Score>> GetByBeatmapIdWithUserDataAsync(int beatmapId, CancellationToken cancellationToken) =>
        Set
            .Where(s => s.BeatmapId == beatmapId)
            .AsSplitQuery()
            .Include(s => s.User)
            .Include(s => s.Beatmap)
            .ThenInclude(b => b.Beatmapset)
            .ToListAsync(cancellationToken);

    public Task<List<IGrouping<int, Score>>> GetByBeatmapIdsAsync(IEnumerable<int> beatmapIds, CancellationToken cancellationToken) =>
        Set.Where(s => beatmapIds.Contains(s.BeatmapId)).GroupBy(s => s.BeatmapId).ToListAsync(cancellationToken);

    public IQueryable<Score> GetAllWithBeatmapAndUserData() => GetAll()
        .AsSplitQuery()
        .Include(s => s.User)
        .Include(s => s.Beatmap)
        .ThenInclude(b => b.Beatmapset);
}
