using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.OsuApi.Enums;

namespace OsuScoreStats.DbService.Repositories;

public class ScoreRepository(ScoreDataContext db) : BaseRepository<Score, ulong>(db), IScoreRepository
{
    public Task<List<Score>> GetByBeatmapIdAsync(int beatmapId, CancellationToken cancellationToken) =>
        Set.Where(s => s.BeatmapId == beatmapId).ToListAsync(cancellationToken);
    
    public Task<List<Score>> GetByBeatmapIdWithUserDataAsync(int beatmapId, Mode mode, CancellationToken cancellationToken) =>
        Set
            .Where(s => s.BeatmapId == beatmapId && s.Mode == mode)
            .AsSplitQuery()
            .Include(s => s.User)
            .ThenInclude(u => u.Country)
            .Include(s => s.Beatmap)
            .ThenInclude(b => b.Beatmapset)
            .OrderBy(s => s.Rank)
            .ToListAsync(cancellationToken);

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
