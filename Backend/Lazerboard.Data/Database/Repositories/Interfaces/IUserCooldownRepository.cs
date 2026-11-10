using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IUserCooldownRepository : IRepository<UserCooldown, int>
{
    Task<UserCooldown?> GetByUserIdAsync(int userId, CancellationToken cancellationToken);
}