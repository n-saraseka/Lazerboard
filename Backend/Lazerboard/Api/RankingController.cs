using Lazerboard.Api.Dtos;
using Lazerboard.Api.Utils;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.Redis.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Api;

[ApiController]
[Route("api/[controller]")]
public class RankingController(IScoreRepository scoreRepository, 
    IUserRepository userRepository, 
    IScoreCacheRepository scoreCacheRepository) : ControllerBase
{
    private readonly TimeSpan _rankingTtl = TimeSpan.FromMinutes(30);
    private readonly BeatmapStatus[] _eligibleStatuses =
        [BeatmapStatus.Ranked, BeatmapStatus.Approved, BeatmapStatus.Loved];
    
    /// <summary>
    /// Get a user ranking by scores count
    /// </summary>
    /// <param name="command">The <see cref="ScoreQueryCommand"/></param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <param name="amount">Amount of <see cref="Score"/>s to return</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>A <see cref="UserRankingResponse"/></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<UserRankingResponse>> GetUserRankingAsync(
        [FromBody] ScoreQueryCommand command,
        [FromQuery] int? page,
        [FromQuery] int? amount,
        CancellationToken ct = default)
    {
        var rankingPage = page ?? 1;
        var rankingAmount = amount ?? 10;
        
        if (rankingAmount > 50 || rankingAmount < 1) return BadRequest($"{nameof(rankingAmount)} must be between 1 and 50");
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

        var clampedCommand = FilterUtils.GetCommandWithClampedRanges(filteredCommand);
        var commandHash = ResponseCachingUtils.GetScoreCommandHashForCache(clampedCommand);
        query = FilterUtils.FilterScoreQuery(query, clampedCommand);
        
        return Ok(await GetRankingsFromQueryAsync(query, rankingAmount, rankingPage, commandHash, false, ct));
    }
    
    /// <summary>
    /// Get an osu!mania millions ranking
    /// </summary>
    /// <param name="command">The <see cref="ScoreQueryCommand"/></param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <param name="amount">Amount of <see cref="UserRanking"/>s to return</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A <see cref="UserRankingResponse"/></returns>
    [HttpPost("millions")]
    [AllowAnonymous]
    public async Task<ActionResult<UserRankingResponse>> GetMillionsRankingAsync(
        [FromBody] ScoreQueryCommand command,
        [FromQuery] int? page,
        [FromQuery] int? amount,
        CancellationToken cancellationToken)
    {
        var rankingPage = page ?? 1;
        var rankingAmount = amount ?? 10;
        
        if (rankingAmount > 50 || rankingAmount < 1) return BadRequest($"{nameof(rankingAmount)} must be between 1 and 50");
        if (command.BeatmapStatus != null && !_eligibleStatuses.Contains(command.BeatmapStatus.Value))
        {
            return BadRequest($"{nameof(command.BeatmapStatus)} must be ranked, approved or loved");
        }
        
        var query = scoreRepository.GetAll().Where(s => s.TotalScore == 1000000 && s.Mode == Mode.Mania);

        var filteredCommand = new ScoreQueryCommand
        {
            ExcludeConverts = command.ExcludeConverts,
            StarRange = command.StarRange,
            CountryCode = command.CountryCode,
            BeatmapStatus = command.BeatmapStatus
        };
        
        var clampedCommand = FilterUtils.GetCommandWithClampedRanges(filteredCommand);
        var commandHash = ResponseCachingUtils.GetScoreCommandHashForCache(clampedCommand);
        query = FilterUtils.FilterScoreQuery(query, clampedCommand);
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
                User = DbToApiObjectUtils.DbUserToApiUser(users.First(u => u.Id == g.UserId))
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