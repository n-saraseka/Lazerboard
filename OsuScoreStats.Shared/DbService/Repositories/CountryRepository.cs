using OsuScoreStats.Shared.DbService.Entities;
using OsuScoreStats.Shared.DbService.Repositories.Interfaces;

namespace OsuScoreStats.Shared.DbService.Repositories;

public class CountryRepository(ScoreDataContext db) : BaseRepository<Country, string>(db), ICountryRepository
{ 
    // Only exists to keep things the same as other repositories for now.
}
