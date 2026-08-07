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
public class ScoresController(IScoreRepository scoreRepository) : ControllerBase
{
    /// <summary>
    /// Get scores
    /// </summary>
    /// <param name="modes">Gameplay modes to get scores from (Osu, Taiko, Fruits, Mania)</param>
    /// <param name="dateStart">Date to begin getting scores from (defaults to today)</param>
    /// <param name="dateEnd">Date to end getting scores from (defaults to today)</param>
    /// <param name="country"><see cref="Country"/> code</param>
    /// <param name="amount">Amount of <see cref="Score"/>s to return</param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <param name="sort">Parameter to sort by</param>
    /// <param name="isDesc">Whether sort is descending or not</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A <see cref="ScoresResponse"/></returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ScoresResponse> GetScoresAsync(
        [FromQuery] Mode[] modes, 
        [FromQuery] DateOnly? dateStart,
        [FromQuery] DateOnly? dateEnd,
        [FromQuery] string? country,
        [FromQuery] int? amount,
        [FromQuery] int? page = 1,
        [FromQuery] string? sort = "pp",
        [FromQuery] bool isDesc = true,
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
        
        if (country != null) query = query.Where(s => s.User.CountryCode == country);

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

    /// <summary>
    /// Get a user ranking by scores count
    /// </summary>
    /// <param name="rankMin">Minimum map rank threshold</param>
    /// <param name="rankMax">Maximum map rank threshold</param>
    /// <param name="modes">Modes to count scores from</param>
    /// <param name="mods">Mods to count scores with</param>
    /// <param name="lenientMode">Whether to allow other mods than <paramref name="mods"/></param>
    /// <param name="ppMin">Minimum PP threshold</param>
    /// <param name="ppMax">Maximum PP threshold</param>
    /// <param name="scoreMin">Minimum TotalScore threshold</param>
    /// <param name="scoreMax">Maximum TotalScore threshold</param>
    /// <param name="countryCode"><see cref="Country"/> to count user scores from</param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <param name="amount">Amount of <see cref="UserRanking"/>s to return</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A <see cref="UserRankingResponse"/></returns>
    [HttpGet("ranking")]
    [AllowAnonymous]
    public async Task<UserRankingResponse> GetUserRankingAsync(
        [FromQuery] int rankMin,
        [FromQuery] int rankMax,
        [FromQuery] Mode[] modes,
        [FromQuery] string[] mods,
        [FromQuery] bool lenientMode,
        [FromQuery] int? ppMin,
        [FromQuery] int? ppMax,
        [FromQuery] int? scoreMin,
        [FromQuery] int? scoreMax,
        [FromQuery] string? countryCode,
        [FromQuery] int? page,
        [FromQuery] int? amount,
        CancellationToken cancellationToken)
    {
        var rankingPage = page == null ? 1 : Math.Max(1, (int)page);
        var rankingAmount = amount == null ? 10 : Math.Min(50, Math.Max((int)amount, 10));
        
        var query = scoreRepository.GetAllWithUserData();

        query = query.Where(s => s.Rank >= rankMin && s.Rank <= rankMax);
        
        if (ppMin != null) query = query.Where(s => s.PP >= ppMin);
        if (ppMax != null) query = query.Where(s => s.PP <= ppMax);
        
        if (scoreMin != null) query = query.Where(s => s.TotalScore >= scoreMin);
        if (scoreMax != null) query = query.Where(s => s.TotalScore <= scoreMax);
        
        query = query.Where(s => modes.Contains(s.Mode));

        switch (lenientMode)
        {
            case true when mods.Length != 0:
                query = query.Where(s => mods.All(a => s.ModAcronyms.Contains(a)));
                break;
            case false:
                query = mods.Length == 0 
                        ? query.Where(s => s.ModAcronyms.Count == 0)
                        : query.Where(s => mods.All(a => s.ModAcronyms.Contains(a)) && s.ModAcronyms.Count == mods.Length);
                break;
        }

        if (countryCode != null) query = query.Where(s => s.User.CountryCode == countryCode);

        var group = query
            .GroupBy(s => s.User)
            .Select(g => new UserRanking 
            {
                User = g.Key,
                ScoresCount = g.Count() 
            })
            .OrderByDescending(s => s.ScoresCount);

        var count = await group.CountAsync(cancellationToken);

        var result = await group
            .Skip((rankingPage - 1) * rankingAmount)
            .Take(rankingAmount)
            .ToListAsync(cancellationToken);
        result = result.OrderByDescending(r => r.ScoresCount).ToList();
        foreach (var userRanking in result)
        {
            userRanking.Rank = result.IndexOf(userRanking) + 1 + (rankingPage - 1) * rankingAmount;
        }

        return new UserRankingResponse
        {
            UserRankings = result,
            Count = count
        };
    }
}