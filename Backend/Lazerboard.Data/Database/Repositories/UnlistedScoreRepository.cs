using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Data.Database.Repositories;

public class UnlistedScoreRepository(ScoreDataContext db) : BaseRepository<UnlistedScore, ulong>(db), IUnlistedScoreRepository
{
    public IQueryable<UnlistedScore> GetByUserId(int userId) => Set
        .AsNoTracking()
        .Where(s => s.UserId == userId);
}