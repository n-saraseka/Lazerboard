using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.DbService.Entities;
namespace OsuScoreStats.DbService.Repositories;

public class UserRepository(ScoreDataContext db) : BaseRepository<User, int>(db), IUserRepository
{
    public Task<User?> GetByIdWithCountryAsync(int id, CancellationToken cancellationToken) => Set
        .Where(u => u.Id == id)
        .AsSplitQuery()
        .Include(u => u.Country)
        .FirstOrDefaultAsync(cancellationToken);
}
