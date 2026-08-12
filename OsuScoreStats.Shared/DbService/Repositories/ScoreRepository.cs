using Microsoft.EntityFrameworkCore;
using OsuScoreStats.Shared.DbService.Entities;
using OsuScoreStats.Shared.DbService.Repositories.Interfaces;
using OsuScoreStats.Shared.OsuApi.Enums;

namespace OsuScoreStats.Shared.DbService.Repositories;

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
            .AsSplitQuery()
            .Include(s => s.User)
            .ThenInclude(u => u.Country)
            .Include(s => s.Beatmap)
            .ThenInclude(b => b.Beatmapset)
            .ToListAsync(cancellationToken);
    
    public Task<int> GetBeatmapScoreCount(int beatmapId, Mode mode, CancellationToken cancellationToken) => Set
        .Where(s => s.BeatmapId == beatmapId && s.Mode == mode).CountAsync(cancellationToken);

    public Task<List<Score>> GetByBeatmapIdsAsync(IEnumerable<int> beatmapIds, CancellationToken cancellationToken) =>
        Set.Where(s => beatmapIds.Contains(s.BeatmapId)).ToListAsync(cancellationToken);

    public IQueryable<Score> GetAllWithBeatmapAndUserData() => GetAll()
        .AsSplitQuery()
        .Include(s => s.User)
        .ThenInclude(u => u.Country)
        .Include(s => s.Beatmap)
        .ThenInclude(b => b.Beatmapset);

    public IQueryable<Score> GetAllWithUserData() => GetAll()
        .AsSplitQuery()
        .Include(s => s.User)
        .ThenInclude(u => u.Country);
}
