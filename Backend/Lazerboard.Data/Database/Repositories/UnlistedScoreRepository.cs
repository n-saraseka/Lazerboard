using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Data.Database.Repositories;

public class UnlistedScoreRepository(ScoreDataContext db) : BaseRepository<UnlistedScore, ulong>(db), IUnlistedScoreRepository
{
    public IQueryable<UnlistedScore> GetByUserId(int userId) => Set
        .AsNoTracking()
        .Where(s => s.UserId == userId);
    
    public IQueryable<UnlistedScore> GetByUserIds(IList<int> userIds) => Set
        .AsNoTracking()
        .Where(s => userIds.Contains(s.UserId))
        .OrderBy(s => s.Id);

    public Task<List<int>> GetAllUsersAsync(CancellationToken ct) => Set
        .AsNoTracking()
        .Select(s => s.UserId)
        .Distinct()
        .ToListAsync(ct);
}