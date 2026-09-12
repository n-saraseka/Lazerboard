using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Data.Database.Repositories;

public class UnlistedScoreRepository(ScoreDataContext db) : BaseRepository<UnlistedScore, ulong>(db), IUnlistedScoreRepository
{
    // Only exists to have things the same as other repositories for now.
}