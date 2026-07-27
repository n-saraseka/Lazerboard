using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.OsuApi.Enums;

namespace OsuScoreStats.ApiMethods;

public class ScoreMethods(IScoreRepository scoreRepository, IUserRepository userRepository)
{
    /// <summary>
    /// Get scores
    /// </summary>
    /// <param name="mode">Gameplay mode (Osu, Taiko, Fruits, Mania)</param>
    /// <param name="dateStart">Date to begin getting scores from (defaults to latest date in scores table)</param>
    /// <param name="dateEnd">Date to end getting scores from (defaults to latest date in scores table)</param>
    /// <param name="country">Country code</param>
    /// <param name="mandatoryMods">An array of mandatory mod acronyms</param>
    /// <param name="optionalMods">An array of optional mod acronyms</param>
    /// <param name="amount">Amount of scores to return</param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <param name="sort">Parameter to sort by</param>
    /// <param name="isDesc">Whether sort is descending or not</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List containing up to 100 highest pp scores at given date</returns>
    public async Task<List<Score>> GetScoresAsync(
        Mode? mode, 
        DateOnly? dateStart,
        DateOnly? dateEnd,
        string? country,
        string[]? mandatoryMods,
        string[]? optionalMods,
        int? amount,
        int? page = 1,
        string? sort = "pp",
        bool isDesc = true,
        CancellationToken ct = default)
    {
        var scoresPage = page == null ? 1 : Math.Max(1, (int)page);
        var scoresAmount = amount == null ? 25 : Math.Min(100, Math.Max((int)amount, 0));

        var query = scoreRepository.GetAll();
        
        var latestDate = await query.MaxAsync(s => s.Date, ct);
        var targetStartDate = dateStart ?? DateOnly.FromDateTime(latestDate);
        var targetEndDate = dateEnd ?? DateOnly.FromDateTime(latestDate);
        query = query.Where(s => 
            DateOnly.FromDateTime(s.Date) >= targetStartDate && DateOnly.FromDateTime(s.Date) <= targetEndDate);
        
        if (mode.HasValue)
            query = query.Where(s => s.Mode == mode.Value);
        
        if (country != null)
        {
            var userIdsThisCountry = await userRepository
                .GetAll()
                .Where(u => u.CountryCode == country)
                .Select(u => u.Id).Distinct()
                .ToListAsync(ct);
            query = query.Where(s => userIdsThisCountry.Contains(s.UserId));
        }
        
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
}