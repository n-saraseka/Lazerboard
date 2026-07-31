using OsuScoreStats.DbService.Entities;

namespace OsuScoreStats.DbService.Repositories.Interfaces;

public interface IUserRepository : IRepository<User, int>
{
    public Task<User?> GetByIdWithCountryAsync(int id, CancellationToken cancellationToken);
}