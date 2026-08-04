using Microsoft.EntityFrameworkCore;
using OsuScoreStats.Api.Dtos;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.OsuApi.Enums;

namespace OsuScoreStats.Api;

public class ScoreMethods(IScoreRepository scoreRepository, IUserRepository userRepository)
{
    /// <summary>
    /// Get scores
    /// </summary>
    /// <param name="modes">Gameplay modes to get scores from (Osu, Taiko, Fruits, Mania)</param>
    /// <param name="dateStart">Date to begin getting scores from (defaults to today)</param>
    /// <param name="dateEnd">Date to end getting scores from (defaults to today)</param>
    /// <param name="country">Country code</param>
    /// <param name="mandatoryMods">An array of mandatory mod acronyms</param>
    /// <param name="optionalMods">An array of optional mod acronyms</param>
    /// <param name="amount">Amount of scores to return</param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <param name="sort">Parameter to sort by</param>
    /// <param name="isDesc">Whether sort is descending or not</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A <see cref="ScoresResponse"/></returns>
    public async Task<ScoresResponse> GetScoresAsync(
        Mode[] modes, 
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

        var query = scoreRepository.GetAllWithBeatmapAndUserData();
        
        query = query.Where(s => modes.Contains(s.Mode));
            
        var targetStartDate = dateStart ?? DateOnly.FromDateTime(DateTime.Today);
        var targetEndDate = dateEnd ?? DateOnly.FromDateTime(DateTime.Today);
        query = query.Where(s => 
            DateOnly.FromDateTime(s.Date) >= targetStartDate && DateOnly.FromDateTime(s.Date) <= targetEndDate);
        
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
        
        var count = await query.CountAsync(ct);
        
        query = query.Skip(scoresAmount * (scoresPage - 1)).Take(scoresAmount);

        var scores = await query.ToListAsync(ct);

        return new ScoresResponse
        {
            Scores = scores,
            Count = count,
        };
    } 
}