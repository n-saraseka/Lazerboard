using Lazerboard.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;

namespace Lazerboard.Api;

[ApiController]
[Route("api/[controller]")]
public class ScoresController(IScoreRepository scoreRepository) : ControllerBase
{
    /// <summary>
    /// Get scores
    /// </summary>
    /// <param name="command">The <see cref="ScoreQueryCommand"/></param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <param name="amount">Amount of <see cref="Score"/>s to return</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>A <see cref="ScoresResponse"/></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> GetScoresAsync(
        [FromBody] ScoreQueryCommand command,
        [FromQuery] int? page,
        [FromQuery] int? amount,
        CancellationToken ct = default)
    {
        var scoresPage = page ?? 1;
        var scoresAmount = amount ?? 25;

        if (scoresAmount > 100) return BadRequest($"{nameof(scoresAmount)} must be less or equal to 100");
        if (command.IncludeMods.Intersect(command.ExcludeMods).Any()) 
            return BadRequest($"{nameof(command.IncludeMods)} must not contain any mods from {nameof(command.ExcludeMods)}");

        var query = scoreRepository.GetAllWithBeatmapAndUserData();
        
        var targetStartDate = command.DateRange[0] ?? DateOnly.FromDateTime(DateTime.Today);
        var targetEndDate = command.DateRange[1] ?? DateOnly.FromDateTime(DateTime.Today);

        var filteredCommand = new ScoreQueryCommand
        {
            Modes = command.Modes,
            DateRange = [targetStartDate, targetEndDate],
            RankRange = command.RankRange,
            PpRange = command.PpRange,
            AccuracyRange = command.AccuracyRange,
            SpeedRange = command.SpeedRange,
            IncludeMods = command.IncludeMods,
            ExcludeMods = command.ExcludeMods,
            LenientMode = command.LenientMode,
            CountryCode = command.CountryCode,
            SortBy = command.SortBy,
            IsDescending = command.IsDescending
        };
        
        query = FilterUtils.FilterScoreQuery(query, filteredCommand);
        
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
    /// <param name="command">The <see cref="ScoreQueryCommand"/></param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <param name="amount">Amount of <see cref="Score"/>s to return</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>A <see cref="UserRankingResponse"/></returns>
    [HttpPost("ranking")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserRankingAsync(
        [FromBody] ScoreQueryCommand command,
        [FromQuery] int? page,
        [FromQuery] int? amount,
        CancellationToken ct = default)
    {
        var rankingPage = page ?? 1;
        var rankingAmount = amount ?? 10;
        
        if (rankingAmount > 50) return BadRequest($"{nameof(rankingAmount)} must be less or equal to 50");
        if (command.IncludeMods.Intersect(command.ExcludeMods).Any()) 
            return BadRequest($"{nameof(command.IncludeMods)} must not contain any mods from {nameof(command.ExcludeMods)}");
        
        var query = scoreRepository.GetAllWithUserData();
        
        var filteredCommand = new ScoreQueryCommand
        {
            Modes = command.Modes,
            RankRange = command.RankRange,
            PpRange = command.PpRange,
            AccuracyRange = command.AccuracyRange,
            SpeedRange = command.SpeedRange,
            IncludeMods = command.IncludeMods,
            ExcludeMods = command.ExcludeMods,
            LenientMode = command.LenientMode,
            CountryCode = command.CountryCode,
        };

        query = FilterUtils.FilterScoreQuery(query, filteredCommand);

        var group = query
            .GroupBy(s => s.UserId)
            .Select(g => new UserRanking 
            {
                User = g.First().User,
                ScoresCount = g.Count() 
            })
            .OrderByDescending(s => s.ScoresCount);

        var count = await group.CountAsync(ct);
        var pages = (int)Math.Ceiling((double)count / rankingAmount);
        if (rankingPage > pages) rankingPage = Math.Max(pages, 1);

        var result = await group
            .Skip((rankingPage - 1) * rankingAmount)
            .Take(rankingAmount)
            .ToListAsync(ct);
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
    /// <param name="command">The <see cref="ScoreQueryCommand"/></param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <param name="amount">Amount of <see cref="UserRanking"/>s to return</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A <see cref="UserRankingResponse"/></returns>
    [HttpPost("millions")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMillionsRankingAsync(
        [FromBody] ScoreQueryCommand command,
        [FromQuery] int? page,
        [FromQuery] int? amount,
        CancellationToken cancellationToken)
    {
        var rankingPage = page ?? 1;
        var rankingAmount = amount ?? 10;
        
        if (rankingAmount > 50) return BadRequest($"{nameof(rankingAmount)} must be less or equal to 50");
        
        var query = scoreRepository.GetAllWithBeatmapAndUserData()
            .Where(s => s.TotalScore == 1000000);

        var filteredCommand = new ScoreQueryCommand
        {
            StarRange = command.StarRange,
            CountryCode = command.CountryCode,
            Modes = [Mode.Mania]
        };
        
        query = FilterUtils.FilterScoreQuery(query, filteredCommand);

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