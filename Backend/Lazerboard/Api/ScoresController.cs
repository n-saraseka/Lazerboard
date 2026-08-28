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
public class ScoresController(IScoreRepository scoreRepository,
    IBeatmapRepository beatmapRepository,
    IUserRepository userRepository) : ControllerBase
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

        var query = scoreRepository.GetAll();
        
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
        
        var beatmaps = await beatmapRepository
            .GetBulkWithBeatmapsetsAsync(scores.Select(s => s.BeatmapId).Distinct().ToList(), ct);
        
        var users = await userRepository.GetBulkAsync(scores.Select(s => s.UserId).Distinct().ToList(), ct);

        scores = scores.Select(s =>
        {
            s.Beatmap = beatmaps.First(b => b.Id == s.BeatmapId);
            s.User = users.First(u => u.Id == s.UserId);
            return s;
        }).ToList();

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
        
        var query = scoreRepository.GetAll();
        
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

        return Ok(await GetRankingsFromQueryAsync(query, rankingAmount, rankingPage, ct));
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
        
        var query = scoreRepository.GetAll().Where(s => s.TotalScore == 1000000);

        var filteredCommand = new ScoreQueryCommand
        {
            StarRange = command.StarRange,
            CountryCode = command.CountryCode,
            Modes = [Mode.Mania]
        };
        
        query = FilterUtils.FilterScoreQuery(query, filteredCommand);

        return Ok(await GetRankingsFromQueryAsync(query, rankingAmount, rankingPage, cancellationToken));
    }

    private async Task<UserRankingResponse> GetRankingsFromQueryAsync(IQueryable<Score> query, 
        int rankingAmount,
        int rankingPage, 
        CancellationToken cancellationToken = default)
    {
        var count = await query.Select(s => s.UserId).Distinct().CountAsync(cancellationToken);
        
        var pages = (int)Math.Ceiling((double)count / rankingAmount);
        if (rankingPage > pages) rankingPage = Math.Max(pages, 1);
        
        var group = await query
            .GroupBy(s => s.UserId)
            .Select(g => new
            {
                g.First().UserId,
                ScoresCount = g.Count()
            })
            .OrderByDescending(r => r.ScoresCount)
            .Skip((rankingPage - 1) * rankingAmount)
            .Take(rankingAmount)
            .ToListAsync(cancellationToken);
        
        var users = await userRepository.GetBulkAsync(group.Select(g => g.UserId), cancellationToken);

        var rankings = group.Select(g => new UserRanking
        {
            Rank = group.IndexOf(g) + 1,
            ScoresCount = g.ScoresCount,
            User = users.First(u => u.Id == g.UserId)
        }).ToList();

        return new UserRankingResponse
        {
            UserRankings = rankings,
            Count = count
        };
    }
}