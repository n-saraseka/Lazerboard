using OsuScoreStats.Data.Database.Entities;

namespace OsuScoreStats.Data.Database.Repositories.Interfaces;

public interface IUserRepository : IRepository<User, int>
{
    public Task<User?> GetByIdWithCountryAsync(int id, CancellationToken cancellationToken);
    public Task<List<User>> SearchAsync(string query, CancellationToken cancellationToken);
}