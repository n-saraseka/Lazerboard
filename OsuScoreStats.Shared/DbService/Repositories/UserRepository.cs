using Microsoft.EntityFrameworkCore;
using OsuScoreStats.Shared.DbService.Entities;
using OsuScoreStats.Shared.DbService.Repositories.Interfaces;

namespace OsuScoreStats.Shared.DbService.Repositories;

public class UserRepository(ScoreDataContext db) : BaseRepository<User, int>(db), IUserRepository
{
    public Task<User?> GetByIdWithCountryAsync(int id, CancellationToken cancellationToken) => Set
        .Where(u => u.Id == id)
        .AsSplitQuery()
        .Include(u => u.Country)
        .FirstOrDefaultAsync(cancellationToken);

    public Task<List<User>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var trimmedQuery = query.Length > 100 ? query.Substring(0, 100) : query;
        trimmedQuery = trimmedQuery.ToLower();
        
        return Set
            .Where(u => u.Username.ToLower().StartsWith(trimmedQuery))
            .Take(25)
            .AsSplitQuery()
            .Include(u => u.Country)
            .ToListAsync(cancellationToken);
    } 
}
