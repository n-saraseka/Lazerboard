using OsuScoreStats.Data.Database.Entities;

namespace OsuScoreStats.Data.Database.Repositories.Interfaces;

public interface IScorePendingDeletionRepository  : IRepository<ScorePendingDeletion, int>
{
    Task<List<ScorePendingDeletion>>  GetByUserIdAsync(int userId);
    Task<List<IGrouping<int, ScorePendingDeletion>>> GetByUserIdsAsync(IList<int> userIds);
}