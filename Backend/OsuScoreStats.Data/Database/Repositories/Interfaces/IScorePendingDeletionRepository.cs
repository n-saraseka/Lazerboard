using OsuScoreStats.Data.Database.Entities;

namespace OsuScoreStats.Data.Database.Repositories.Interfaces;

public interface IScorePendingDeletionRepository  : IRepository<ScorePendingDeletion, int>
{
    Task<List<ScorePendingDeletion>> GetAllWithUserData();
}