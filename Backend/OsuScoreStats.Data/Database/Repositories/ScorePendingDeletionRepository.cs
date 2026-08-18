using Microsoft.EntityFrameworkCore;
using OsuScoreStats.Data.Database.Entities;
using OsuScoreStats.Data.Database.Repositories.Interfaces;

namespace OsuScoreStats.Data.Database.Repositories;

public class ScorePendingDeletionRepository(ScoreDataContext db) : BaseRepository<ScorePendingDeletion, int>(db), IScorePendingDeletionRepository
{
    public Task<List<ScorePendingDeletion>> GetAllWithUserData() => GetAll()
        .AsSplitQuery()
        .Include(s => s.Score)
        .ThenInclude(s => s.User)
        .ToListAsync();
}