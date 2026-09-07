using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Data.Database.Repositories;

public class UserRepository(ScoreDataContext db) : BaseRepository<User, int>(db), IUserRepository
{
    public Task<List<User>> GetBulkWithCountriesAsync(IList<int> ids, CancellationToken cancellationToken) => Set
        .AsNoTracking()
        .Where(u => ids.Contains(u.Id))
        .Include(u => u.Country)
        .ToListAsync(cancellationToken);

    public Task<User?> GetByIdWithCountryAsync(int id, CancellationToken cancellationToken) => Set
        .AsNoTracking()
        .Where(u => u.Id == id)
        .Include(u => u.Country)
        .FirstOrDefaultAsync(cancellationToken);

    public Task<List<User>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var trimmedQuery = query.Length > 100 ? query.Substring(0, 100) : query;
        trimmedQuery = trimmedQuery.ToLower();
        
        return Set
            .AsNoTracking()
            .Where(u => u.Username.ToLower().StartsWith(trimmedQuery) && u.CountryCode != null)
            .Take(25)
            .Include(u => u.Country)
            .OrderBy(u => u.Username.Length)
            .ThenByDescending(u => u.Username)
            .ToListAsync(cancellationToken);
    } 
}
