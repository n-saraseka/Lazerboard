using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsuScoreStats.Api.Dtos;
using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.OsuApi.Enums;

namespace OsuScoreStats.Api;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IScoreRepository scoreRepository, IUserRepository userRepository) : ControllerBase
{
    /// <summary>
    /// Get user scores
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="modes">Gameplay modes to get scores from (Osu, Taiko, Fruits, Mania)</param>
    /// <param name="dateStart">Date to begin getting scores from (defaults to Unix epoch)</param>
    /// <param name="dateEnd">Date to end getting scores from (defaults to latest date in scores table)</param>
    /// <param name="mandatoryMods">An array of mandatory mod acronyms</param>
    /// <param name="optionalMods">An array of optional mod acronyms</param>
    /// <param name="amount">Amount of scores to return</param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <param name="sort">Parameter to sort by</param>
    /// <param name="isDesc">Whether sort is descending or not</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A <see cref="ScoresResponse"/></returns>
    [HttpGet("{userId:int}/scores")]
    [AllowAnonymous]
    public async Task<ScoresResponse> GetUserScoresAsync(
        int userId,
        [FromQuery] Mode[] modes,
        [FromQuery] DateOnly? dateStart,
        [FromQuery] DateOnly? dateEnd,
        [FromQuery] string[]? mandatoryMods,
        [FromQuery] string[]? optionalMods,
        [FromQuery] int? amount,
        [FromQuery] int? page = 1,
        [FromQuery] string? sort = "pp",
        [FromQuery] bool isDesc = true,
        CancellationToken ct = default)
    {
        var scoresPage = page == null ? 1 : Math.Max(1, (int)page);
        var scoresAmount = (amount == null) ? 25 : Math.Min(100, Math.Max((int)amount, 0));
        var query = scoreRepository.GetAllWithBeatmapAndUserData().Where(s => s.UserId == userId);
        
        var latestDate = await query.MaxAsync(s => s.Date, ct);
        var targetStartDate = dateStart ?? DateOnly.FromDateTime(DateTime.UnixEpoch);
        var targetEndDate = dateEnd ?? DateOnly.FromDateTime(latestDate);
        query = query.Where(s => 
            DateOnly.FromDateTime(s.Date) >= targetStartDate && DateOnly.FromDateTime(s.Date) <= targetEndDate);
        
        query = query.Where(s => modes.Contains(s.Mode));
        
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
        
        query = query
            .Skip(scoresAmount * (scoresPage - 1)).Take(scoresAmount);

        var scores = await query.ToListAsync(ct);

        return new ScoresResponse
        {
            Scores = scores,
            Count = count,
        };
    }
}