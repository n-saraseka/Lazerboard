using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.DbService.Entities;
namespace OsuScoreStats.DbService.Repositories;

public class UserRepository(ScoreDataContext db) : BaseRepository<User, int>(db), IUserRepository
{ 
    // Only exists to keep things the same as other repositories for now.
}
