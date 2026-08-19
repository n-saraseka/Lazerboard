using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Data.Database.Repositories;

public class ScorePendingDeletionRepository(ScoreDataContext db) : BaseRepository<ScorePendingDeletion, int>(db), IScorePendingDeletionRepository
{
    public Task<List<ScorePendingDeletion>> GetAllWithUserData() => GetAll()
        .AsSplitQuery()
        .Include(s => s.Score)
        .ThenInclude(s => s.User)
        .ToListAsync();
}