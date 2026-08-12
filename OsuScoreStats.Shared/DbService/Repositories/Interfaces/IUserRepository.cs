using OsuScoreStats.Shared.DbService.Entities;

namespace OsuScoreStats.Shared.DbService.Repositories.Interfaces;

public interface IUserRepository : IRepository<User, int>
{
    public Task<User?> GetByIdWithCountryAsync(int id, CancellationToken cancellationToken);
    public Task<List<User>> SearchAsync(string query, CancellationToken cancellationToken);
}