using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories.Interfaces;

namespace OsuScoreStats.DbService.Repositories;

public class ScoreRepository(DbContext db) : BaseRepository<Score, ulong>(db), IScoreRepository
{
    public Task<List<Score>> GetByBeatmapIdAsync(int beatmapId, CancellationToken cancellationToken) =>
        Set.Where(s => s.BeatmapId == beatmapId).ToListAsync(cancellationToken);

    public Task<List<IGrouping<int, Score>>> GetByBeatmapIdsAsync(IEnumerable<int> beatmapIds, CancellationToken cancellationToken) =>
        Set.Where(s => beatmapIds.Contains(s.BeatmapId)).GroupBy(s => s.BeatmapId).ToListAsync(cancellationToken);
}
