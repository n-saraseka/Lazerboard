using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Data.Database.Repositories;

public class UserCooldownRepository(ScoreDataContext db) : BaseRepository<UserCooldown, int>(db), IUserCooldownRepository
{
    public Task<UserCooldown?> GetByUserIdAsync(int userId, CancellationToken cancellationToken) => Set
        .Where(c => c.UserId == userId)
        .OrderByDescending(c => c.Id)
        .FirstOrDefaultAsync(cancellationToken);
}