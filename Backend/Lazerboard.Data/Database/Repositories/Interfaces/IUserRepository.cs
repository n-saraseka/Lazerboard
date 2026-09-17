using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IUserRepository : IRepository<User, int>
{
    Task<List<User>> GetBulkWithCountriesAsync(IList<int> ids, CancellationToken cancellationToken);
    Task<User?> GetByIdWithCountryAsync(int id, CancellationToken cancellationToken);
    Task<List<User>> SearchAsync(string query, CancellationToken cancellationToken);
    Task<User?> GetLatestScannedUserAsync(CancellationToken cancellationToken = default);
}