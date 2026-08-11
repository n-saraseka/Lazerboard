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

    public Task<List<User>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var trimmedQuery = query.Trim().Take(100).ToArray();
        
        return Set
            .Where(u => u.Username.StartsWith(trimmedQuery))
            .ToListAsync(cancellationToken);
    } 
}
