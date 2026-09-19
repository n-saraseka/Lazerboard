using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IUnlistedScoreRepository  : IRepository<UnlistedScore, ulong>
{
    IQueryable<UnlistedScore> GetByUserId(int userId);
    IQueryable<UnlistedScore> GetByUserIds(IList<int> userIds);
}