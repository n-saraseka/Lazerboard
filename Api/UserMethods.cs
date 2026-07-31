using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.OsuApi.Enums;

namespace OsuScoreStats.Api;

public class UserMethods(IScoreRepository scoreRepository, IUserRepository userRepository)
{
    /// <summary>
    /// Get user data from the API
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated APIUser object (or null)</returns>
    public async Task<User?> GetUserAsync(int userId, CancellationToken ct = default) => 
        await userRepository.GetByIdAsync(userId, ct);
    
    /// <summary>
    /// Get users data from the API
    /// </summary>
    /// <param name="userIds">Array containing user IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List containing populated APIUser objects</returns>
    public async Task<List<User>> GetUsersAsync(int[] userIds, CancellationToken ct = default) => 
        await userRepository.GetBulkAsync(userIds, ct);
    
    /// <summary>
    /// Get user scores
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="mode">Gameplay mode (Osu, Taiko, Fruits, Mania)</param>
    /// <param name="mandatoryMods">An array of mandatory mod acronyms</param>
    /// <param name="optionalMods">An array of optional mod acronyms</param>
    /// <param name="amount">Amount of scores to return</param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <param name="sort">Parameter to sort by</param>
    /// <param name="isDesc">Whether sort is descending or not</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List containing up to 100 highest pp scores at given date</returns>
    public async Task<List<Score>> GetUserScoresAsync(
        int userId,
        Mode? mode,
        string[]? mandatoryMods,
        string[]? optionalMods,
        int? amount,
        int? page = 1,
        string? sort = "pp",
        bool isDesc = true,
        CancellationToken ct = default)
    {
        var scoresPage = page == null ? 1 : Math.Max(1, (int)page);
        var scoresAmount = (amount == null) ? 25 : Math.Min(100, Math.Max((int)amount, 0));
        var query = scoreRepository.GetAll().Where(s => s.UserId == userId);
        
        if (mode.HasValue)
            query = query.Where(s => s.Mode == mode.Value);
        
        if (mandatoryMods?.Length > 0)
            query = query.Where(s =>
                mandatoryMods.All(m => s.ModAcronyms.Contains(m)) &&
                s.ModAcronyms.All(m => mandatoryMods.Contains(m)));
        if (optionalMods?.Length > 0)
            query = query.Where(s =>
                s.ModAcronyms.All(m => optionalMods.Contains(m)));

        switch (sort)
        {
            case "totalScore":
                query = (isDesc) ? query.OrderByDescending(s => s.TotalScore) : query.OrderBy(s => s.TotalScore);
                break;
            case "classicTotalScore":
                query = (isDesc) ? query.OrderByDescending(s => s.ClassicTotalScore) : query.OrderBy(s => s.ClassicTotalScore);
                break;
            case "date":
                query = (isDesc) ? query.OrderByDescending(s => s.Date) : query.OrderBy(s => s.Date);
                break;
            default:
                query = (isDesc) ? query.OrderByDescending(s => s.PP) : query.OrderBy(s => s.PP);
                break;
        }
        
        query = query.Skip(scoresAmount * (scoresPage - 1)).Take(scoresAmount);
        
        return await query.ToListAsync(ct);
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
        var query = scoreRepository.GetAll().Where(s => s.UserId == userId);
        
        if (mode.HasValue)
            query = query.Where(s => s.Mode == mode.Value);

        return await query.CountAsync(ct);
    }
}