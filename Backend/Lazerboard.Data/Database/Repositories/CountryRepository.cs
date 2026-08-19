using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;

namespace Lazerboard.Data.Database.Repositories;

public class CountryRepository(ScoreDataContext db) : BaseRepository<Country, string>(db), ICountryRepository
{ 
    // Only exists to keep things the same as other repositories for now.
}
