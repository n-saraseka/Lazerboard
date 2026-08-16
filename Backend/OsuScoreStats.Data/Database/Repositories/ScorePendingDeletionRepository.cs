using Microsoft.EntityFrameworkCore;
using OsuScoreStats.Data.Database.Entities;
using OsuScoreStats.Data.Database.Repositories.Interfaces;

namespace OsuScoreStats.Data.Database.Repositories;

public class ScorePendingDeletionRepository(ScoreDataContext db) : BaseRepository<ScorePendingDeletion, int>(db), IScorePendingDeletionRepository
{
    public Task<List<ScorePendingDeletion>> GetByUserIdAsync(int userId) =>
        Set
            .AsSplitQuery()
            .Include(s => s.Score)
            .Where(s => s.Score.UserId == userId)
            .ToListAsync();
    
    public Task<List<IGrouping<int, ScorePendingDeletion>>> GetByUserIdsAsync(IList<int> userIds) =>
        Set
            .AsSplitQuery()
            .Include(s => s.Score)
            .Where(s => userIds.Contains(s.Score.UserId))
            .GroupBy(s => s.Score.UserId)
            .ToListAsync();
}