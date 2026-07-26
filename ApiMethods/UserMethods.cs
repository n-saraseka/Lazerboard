using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories;
using OsuScoreStats.OsuApi.Enums;

namespace OsuScoreStats.ApiMethods;

public class UserMethods(IDbContextFactory<ScoreDataContext> dbContextFactory)
{
    /// <summary>
    /// Get user data from the API
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated User object (or null)</returns>
    public async Task<User?> GetUserAsync(int userId, CancellationToken ct = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var userRepository = new UserRepository(dbContext);
        
        return await userRepository.GetByIdAsync(userId, ct);
    }
    
    /// <summary>
    /// Get users data from the API
    /// </summary>
    /// <param name="userIds">Array containing user IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List containing populated User objects</returns>
    public async Task<List<User>> GetUsersAsync(int[] userIds, CancellationToken ct = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var userRepository = new UserRepository(dbContext);
        
        return await userRepository.GetBulkAsync(userIds, ct);
    }

    /// <summary>
    /// Get count of scores set by user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="mode">Gameplay mode (osu, taiko, fruits, mania)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Count of scores set by user</returns>
    public async Task<int> GetUserScoresCountAsync(int userId, Mode? mode, CancellationToken ct)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var scoreRepository = new ScoreRepository(dbContext);
        
        var query = scoreRepository.GetAll().Where(s => s.UserId == userId);
        
        if (mode.HasValue)
            query = query.Where(s => s.Mode == mode.Value);

        return await query.CountAsync(ct);
    }
}