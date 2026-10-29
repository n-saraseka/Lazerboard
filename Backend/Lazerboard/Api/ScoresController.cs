using Lazerboard.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.Redis.Repositories.Interfaces;

namespace Lazerboard.Api;

[ApiController]
[Route("api/[controller]")]
public class ScoresController(IScoreRepository scoreRepository,
    IBeatmapRepository beatmapRepository,
    IUserRepository userRepository,
    IScoreCacheRepository scoreCacheRepository) : ControllerBase
{
    private readonly TimeSpan _scoresTtl = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _rankingTtl = TimeSpan.FromMinutes(30);
    private readonly BeatmapStatus[] _eligibleStatuses =
        [BeatmapStatus.Ranked, BeatmapStatus.Approved, BeatmapStatus.Loved];
    
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
        if (command.BeatmapStatus != null && !_eligibleStatuses.Contains(command.BeatmapStatus.Value))
        {
            return BadRequest($"{nameof(command.BeatmapStatus)} must be ranked, approved or loved");
        }

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
            IsDescending = command.IsDescending,
            ExcludeConverts = command.ExcludeConverts,
            BeatmapStatus = command.BeatmapStatus
        };
        
        query = FilterUtils.FilterScoreQuery(query, filteredCommand);

        var commandHash = ResponseCachingUtils.GetScoreCommandHashForCache(filteredCommand);
        var scoresQueryString = ResponseCachingUtils.GetScoreQueryKeyForCache(commandHash, amount, page);
        var scoresResponseString = $"globalscores-{scoresQueryString}";
        
        var scoresResponseCache = await scoreCacheRepository.GetSerializedResponseAsync(scoresResponseString);
        
        if (scoresResponseCache is null)
        {
            var count = await query.CountAsync(ct);
            var pages = (int)Math.Ceiling((double)count / scoresAmount);
            if (scoresPage > pages) scoresPage = Math.Max(pages, 1);
            
            scoresQueryString = ResponseCachingUtils.GetScoreQueryKeyForCache(commandHash, amount, scoresPage);
            scoresResponseString = $"globalscores-{scoresQueryString}";
            
            query = query
                .Skip(scoresAmount * (scoresPage - 1)).Take(scoresAmount);
            var scores = await query.ToListAsync(ct);
            
            var beatmaps = await beatmapRepository
                .GetBulkWithBeatmapsetsAsync(scores.Select(s => s.BeatmapId).Distinct().ToList(), ct);
            var users = await userRepository.GetBulkWithCountriesAsync(scores.Select(s => s.UserId).Distinct().ToList(), ct);

            scores = scores.Select(s =>
            {
                s.Beatmap = beatmaps.First(b => b.Id == s.BeatmapId);
                s.User = users.First(u => u.Id == s.UserId);
                return s;
            }).ToList();

            var scoresResponse = new ScoresResponse
            {
                Scores = scores,
                Count = count
            };

            var serializedString = ResponseCachingUtils.GetSerializedScoresResponse(scoresResponse);
            
            // Only cache the first 5 or the last 5 pages.
            if (page <= 5 || pages - scoresPage <= 5)
            {
                await scoreCacheRepository.SetSerializedResponseAsync(scoresResponseString, serializedString, _scoresTtl);
            }
            
            return Ok(scoresResponse);
        }
        else
        {
            var scoresResponse = ResponseCachingUtils.GetDeserializedScoresResponse(scoresResponseCache);
            return Ok(scoresResponse);
        }
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
        if (command.BeatmapStatus != null && !_eligibleStatuses.Contains(command.BeatmapStatus.Value))
        {
            return BadRequest($"{nameof(command.BeatmapStatus)} must be ranked, approved or loved");
        }
        
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
            ExcludeConverts = command.ExcludeConverts,
            BeatmapStatus = command.BeatmapStatus
        };

        var commandHash = ResponseCachingUtils.GetScoreCommandHashForCache(filteredCommand);
        query = FilterUtils.FilterScoreQuery(query, filteredCommand);
        return Ok(await GetRankingsFromQueryAsync(query, rankingAmount, rankingPage, commandHash, false, ct));
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
            ExcludeConverts = command.ExcludeConverts,
            StarRange = command.StarRange,
            CountryCode = command.CountryCode,
            Modes = [Mode.Mania]
        };
        
        var commandHash = ResponseCachingUtils.GetScoreCommandHashForCache(filteredCommand);
        query = FilterUtils.FilterScoreQuery(query, filteredCommand);
        return Ok(await GetRankingsFromQueryAsync(query, rankingAmount, rankingPage, commandHash, true, cancellationToken));
    }

    private async Task<UserRankingResponse?> GetRankingsFromQueryAsync(IQueryable<Score> query, 
        int rankingAmount,
        int rankingPage,
        string commandHash,
        bool isManiaMillions,
        CancellationToken cancellationToken = default)
    {
        var scoresQueryString = ResponseCachingUtils.GetScoreQueryKeyForCache(commandHash, rankingAmount, rankingPage);
        var scoresResponseString = isManiaMillions
            ? $"maniamillionsranking-{scoresQueryString}" 
            : $"userranking-{scoresQueryString}";
        var rankingResponseCache = await scoreCacheRepository.GetSerializedResponseAsync(scoresResponseString);

        if (rankingResponseCache == null)
        {
            var count = await query.Select(s => s.UserId).Distinct().CountAsync(cancellationToken);

            var pages = (int)Math.Ceiling((double)count / rankingAmount);
            if (rankingPage > pages) rankingPage = Math.Max(pages, 1);
            
            scoresQueryString = ResponseCachingUtils.GetScoreQueryKeyForCache(commandHash, rankingAmount, rankingPage);
            scoresResponseString = isManiaMillions
                ? $"maniamillionsranking-{scoresQueryString}" 
                : $"userranking-{scoresQueryString}";

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

            var users = await userRepository.GetBulkWithCountriesAsync(group.Select(g => g.UserId).ToList(),
                cancellationToken);

            var rankings = group.Select(g => new UserRanking
            {
                Rank = (rankingPage - 1) * rankingAmount + group.IndexOf(g) + 1,
                ScoresCount = g.ScoresCount,
                User = users.First(u => u.Id == g.UserId)
            }).ToList();
            
            var response = new UserRankingResponse
            {
                UserRankings = rankings,
                Count = count
            };
            
            var serializedString = ResponseCachingUtils.GetSerializedUserRankingResponse(response);
            
            // Only cache the first 5 or the last 5 pages.
            if (rankingPage <= 5 || pages - rankingPage <= 5)
            {
                await scoreCacheRepository.SetSerializedResponseAsync(scoresResponseString, serializedString, _rankingTtl);
            }

            return response;
        }
        else
        {
            var response = ResponseCachingUtils.GetDeserializedUserRankingResponse(rankingResponseCache);
            return response;
        }
    }
}