using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.DbService.Entities;
namespace OsuScoreStats.DbService.Repositories;

public class CountryRepository(ScoreDataContext db) : BaseRepository<Country, string>(db), ICountryRepository
{ 
    // Only exists to keep things the same as other repositories for now.
}
