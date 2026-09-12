using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IUnlistedScoreRepository  : IRepository<UnlistedScore, ulong>
{
    // Only exists to have things the same as other repositories for now.
}