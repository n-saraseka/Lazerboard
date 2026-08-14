using OsuScoreStats.DbService.Entities;

namespace OsuScoreStats.DbService.Repositories.Interfaces;

public interface ICountryRepository : IRepository<Country, string>
{
    // Only exists to keep things the same as other repositories for now.
}