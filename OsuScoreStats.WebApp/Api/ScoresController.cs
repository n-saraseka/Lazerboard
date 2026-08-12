using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsuScoreStats.Api.Dtos;
using OsuScoreStats.Shared.DbService.Entities;
using OsuScoreStats.Shared.DbService.Repositories.Interfaces;
using OsuScoreStats.Shared.OsuApi.Enums;

namespace OsuScoreStats.Api;

[ApiController]
[Route("api/[controller]")]
public class ScoresController(IScoreRepository scoreRepository) : ControllerBase
{
    /// <summary>
    /// Get scores
    /// </summary>
    /// <param name="modes">Gameplay modes to get scores from (Osu, Taiko, Fruits, Mania)</param>
    /// <param name="rankMin">Minimum map rank threshold</param>
    /// <param name="rankMax">Maximum map rank threshold</param>
    /// <param name="ppMin">Minimum PP threshold</param>
    /// <param name="ppMax">Maximum PP threshold</param>
    /// <param name="accMin">Minimum accuracy threshold</param>
    /// <param name="accMax">Maximum accuracy threshold</param>
    /// <param name="speedMin">Minimum speed threshold</param>
    /// <param name="speedMax">Maximum speed threshold</param>
    /// <param name="includeMods">Mod acronyms that should be included in all scores</param>
    /// <param name="lenientMode">Whether to allow other mods than <paramref name="includeMods"/></param>
    /// <param name="excludeMods">Mod acronyms that should be excluded from all scores</param>
    /// <param name="dateMin">Date to begin getting scores from (defaults to today)</param>
    /// <param name="dateMax">Date to end getting scores from (defaults to today)</param>
    /// <param name="countryCode"><see cref="Country"/> code</param>
    /// <param name="amount">Amount of <see cref="Score"/>s to return</param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <param name="sort">Parameter to sort by</param>
    /// <param name="isDesc">Whether sort is descending or not</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A <see cref="ScoresResponse"/></returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetScoresAsync(
        [FromQuery] Mode[] modes,
        [FromQuery] int? rankMin,
        [FromQuery] int? rankMax,
        [FromQuery] int? ppMin,
        [FromQuery] int? ppMax,
        [FromQuery] double? accMin,
        [FromQuery] double? accMax,
        [FromQuery] double? speedMin,
        [FromQuery] double? speedMax,
        [FromQuery] string[] includeMods,
        [FromQuery] bool lenientMode,
        [FromQuery] string[] excludeMods,
        [FromQuery] DateOnly? dateMin,
        [FromQuery] DateOnly? dateMax,
        [FromQuery] string? countryCode,
        [FromQuery] int? amount,
        [FromQuery] int? page = 1,
        [FromQuery] string? sort = "pp",
        [FromQuery] bool isDesc = true,
        CancellationToken ct = default)
    {
        var scoresPage = page ?? 1;
        var scoresAmount = amount ?? 25;

        if (scoresAmount > 100) return BadRequest($"{nameof(scoresAmount)} must be less or equal to 100");
        if (includeMods.Intersect(excludeMods).Any()) return BadRequest($"{nameof(includeMods)} must not contain any mods from {nameof(excludeMods)}");

        var query = scoreRepository.GetAllWithBeatmapAndUserData();
        
        query = query.Where(s => modes.Contains(s.Mode));
            
        var targetStartDate = dateMin ?? DateOnly.FromDateTime(DateTime.Today);
        var targetEndDate = dateMax ?? DateOnly.FromDateTime(DateTime.Today);

        var command = new ScoreQueryCommand
        {
            Modes = modes,
            DateRange = [targetStartDate, targetEndDate],
            RankRange = [rankMin, rankMax],
            PpRange = [ppMin, ppMax],
            AccuracyRange = [accMin, accMax],
            SpeedRange = [speedMin, speedMax],
            IncludeMods = includeMods,
            ExcludeMods = excludeMods,
            LenientMode = lenientMode,
            CountryCode = countryCode,
            SortBy = sort,
            IsDescending = isDesc
        };
        
        query = FilterUtils.FilterScoreQuery(query, command);
        
        var count = await query.CountAsync(ct);
        var pages = (int)Math.Ceiling((double)count / scoresAmount);
        if (scoresPage > pages) scoresPage = Math.Max(pages, 1);
        
        query = query.Skip(scoresAmount * (scoresPage - 1)).Take(scoresAmount);

        var scores = await query.ToListAsync(ct);

        return Ok(new ScoresResponse
        {
            Scores = scores,
            Count = count,
        });
    }

    /// <summary>
    /// Get a user ranking by scores count
    /// </summary>
    /// <param name="modes">Modes to count scores from</param>
    /// <param name="rankMin">Minimum map rank threshold</param>
    /// <param name="rankMax">Maximum map rank threshold</param>
    /// <param name="ppMin">Minimum PP threshold</param>
    /// <param name="ppMax">Maximum PP threshold</param>
    /// <param name="accMin">Minimum accuracy threshold</param>
    /// <param name="accMax">Maximum accuracy threshold</param>
    /// <param name="speedMin">Minimum speed threshold</param>
    /// <param name="speedMax">Maximum speed threshold</param>
    /// <param name="includeMods">Mod acronyms that should be included in all scores</param>
    /// <param name="lenientMode">Whether to allow other mods than <paramref name="includeMods"/></param>
    /// <param name="excludeMods">Mod acronyms that should be excluded from all scores</param>
    /// <param name="countryCode"><see cref="Country"/> to count user scores from</param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <param name="amount">Amount of <see cref="UserRanking"/>s to return</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A <see cref="UserRankingResponse"/></returns>
    [HttpGet("ranking")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserRankingAsync(
        [FromQuery] Mode[] modes,
        [FromQuery] int rankMin,
        [FromQuery] int rankMax,
        [FromQuery] int? ppMin,
        [FromQuery] int? ppMax,
        [FromQuery] double? accMin,
        [FromQuery] double? accMax,
        [FromQuery] double? speedMin,
        [FromQuery] double? speedMax,
        [FromQuery] string[] includeMods,
        [FromQuery] bool lenientMode,
        [FromQuery] string[] excludeMods,
        [FromQuery] string? countryCode,
        [FromQuery] int? page,
        [FromQuery] int? amount,
        CancellationToken cancellationToken)
    {
        var rankingPage = page ?? 1;
        var rankingAmount = amount ?? 10;
        
        if (rankingAmount > 50) return BadRequest($"{nameof(rankingAmount)} must be less or equal to 50");
        if (includeMods.Intersect(excludeMods).Any()) return BadRequest($"{nameof(includeMods)} must not contain any mods from {nameof(excludeMods)}");
        
        var query = scoreRepository.GetAllWithUserData();
        
        var command = new ScoreQueryCommand
        {
            Modes = modes,
            RankRange = [rankMin, rankMax],
            PpRange = [ppMin, ppMax],
            AccuracyRange = [accMin, accMax],
            SpeedRange = [speedMin, speedMax],
            IncludeMods = includeMods,
            ExcludeMods = excludeMods,
            LenientMode = lenientMode,
            CountryCode = countryCode,
        };

        query = FilterUtils.FilterScoreQuery(query, command);

        var group = query
            .GroupBy(s => s.User)
            .Select(g => new UserRanking 
            {
                User = g.Key,
                ScoresCount = g.Count() 
            })
            .OrderByDescending(s => s.ScoresCount);

        var count = await group.CountAsync(cancellationToken);
        var pages = (int)Math.Ceiling((double)count / rankingAmount);
        if (rankingPage > pages) rankingPage = Math.Max(pages, 1);

        var result = await group
            .Skip((rankingPage - 1) * rankingAmount)
            .Take(rankingAmount)
            .ToListAsync(cancellationToken);
        result = result.OrderByDescending(r => r.ScoresCount).ToList();
        foreach (var userRanking in result)
        {
            userRanking.Rank = result.IndexOf(userRanking) + 1 + (rankingPage - 1) * rankingAmount;
        }

        return Ok(new UserRankingResponse
        {
            UserRankings = result,
            Count = count
        });
    }
    
    /// <summary>
    /// Get a user ranking by count of 1 million scores in <see cref="Mode.Mania"/>
    /// </summary>
    /// <param name="minStars">Minimum beatmap star rating</param>
    /// <param name="maxStars">Maximum beatmap star rating</param>
    /// <param name="countryCode"><see cref="Country"/> to count user scores from</param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <param name="amount">Amount of <see cref="UserRanking"/>s to return</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A <see cref="UserRankingResponse"/></returns>
    [HttpGet("millions")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMillionsRankingAsync(
        [FromQuery] double? minStars,
        [FromQuery] double? maxStars,
        [FromQuery] string? countryCode,
        [FromQuery] int? page,
        [FromQuery] int? amount,
        CancellationToken cancellationToken)
    {
        var rankingPage = page ?? 1;
        var rankingAmount = amount ?? 10;
        
        if (rankingAmount > 50) return BadRequest($"{nameof(rankingAmount)} must be less or equal to 50");
        
        var query = scoreRepository.GetAllWithBeatmapAndUserData()
            .Where(s => s.Mode == Mode.Mania && s.TotalScore == 1000000);

        var command = new ScoreQueryCommand
        {
            StarRange = [minStars, maxStars],
            CountryCode = countryCode,
        };
        
        query = FilterUtils.FilterScoreQuery(query, command);

        var group = query
            .GroupBy(s => s.User)
            .Select(g => new UserRanking 
            {
                User = g.Key,
                ScoresCount = g.Count() 
            })
            .OrderByDescending(s => s.ScoresCount);

        var count = await group.CountAsync(cancellationToken);
        var pages = (int)Math.Ceiling((double)count / rankingAmount);
        if (rankingPage > pages) rankingPage = Math.Max(pages, 1);

        var result = await group
            .Skip((rankingPage - 1) * rankingAmount)
            .Take(rankingAmount)
            .ToListAsync(cancellationToken);
        result = result.OrderByDescending(r => r.ScoresCount).ToList();
        foreach (var userRanking in result)
        {
            userRanking.Rank = result.IndexOf(userRanking) + 1 + (rankingPage - 1) * rankingAmount;
        }

        return Ok(new UserRankingResponse
        {
            UserRankings = result,
            Count = count
        });
    }
}