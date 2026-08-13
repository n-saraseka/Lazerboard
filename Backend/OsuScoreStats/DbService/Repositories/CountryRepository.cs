using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories.Interfaces;

namespace OsuScoreStats.DbService.Repositories;

public class CountryRepository(ScoreDataContext db) : BaseRepository<Country, string>(db), ICountryRepository
{ 
    // Only exists to keep things the same as other repositories for now.
}
