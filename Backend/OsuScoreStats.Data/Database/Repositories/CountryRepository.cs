using OsuScoreStats.Data.Database.Entities;
using OsuScoreStats.Data.Database.Repositories.Interfaces;

namespace OsuScoreStats.Data.Database.Repositories;

public class CountryRepository(ScoreDataContext db) : BaseRepository<Country, string>(db), ICountryRepository
{ 
    // Only exists to keep things the same as other repositories for now.
}
