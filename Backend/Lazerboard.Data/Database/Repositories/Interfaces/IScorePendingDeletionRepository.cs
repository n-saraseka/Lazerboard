using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IScorePendingDeletionRepository  : IRepository<ScorePendingDeletion, int>
{
    Task<List<ScorePendingDeletion>> GetAllWithUserData();
}