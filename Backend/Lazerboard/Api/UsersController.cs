using Lazerboard.Api.Dtos;
using Lazerboard.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.Redis.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication;

namespace Lazerboard.Api;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IScoreRepository scoreRepository, 
    IUserRepository userRepository,
    IBeatmapRepository beatmapRepository,
    IScoreCacheRepository scoreCacheRepository,
    ILogger<UsersController> logger) : ControllerBase
{
    private readonly TimeSpan _scoresTtl = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _statsTtl = TimeSpan.FromMinutes(5);
    private readonly BeatmapStatus[] _eligibleStatuses =
        [BeatmapStatus.Ranked, BeatmapStatus.Approved, BeatmapStatus.Loved];

    private int UserId => int.Parse(HttpContext.User.Identity!.Name!);

    /// <summary>
    /// Get authorized user's data
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns><see cref="User"/> or null</returns>
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("me")]
    public async Task<ActionResult<ApiUser>> GetSelf(CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(UserId, ct);

        if (user is null)
        {
            logger.Log(LogLevel.Information, $"User {UserId} not found, logging out...");
            await HttpContext.SignOutAsync("InternalCookies");

            return Forbid();
        }

        if (!user.UserGroups.Contains(UserGroup.RegisteredUser))
        {
            user.UserGroups.Remove(UserGroup.User);
            user.UserGroups.Add(UserGroup.RegisteredUser);
        }
        
        if (!user.RegisteredAt.HasValue)
        {
            user.RegisteredAt = DateTimeOffset.UtcNow;
            userRepository.Update(user);
            await userRepository.SaveChangesAsync(ct);
            logger.Log(LogLevel.Information, "User {UserId} has registered", UserId);
        }

        return Ok(DbToApiObjectUtils.DbUserToApiUser(user));
    }
    
    /// <summary>
    /// Get user scores
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="command">The <see cref="ScoreQueryCommand"/></param>
    /// <param name="amount">Amount of scores to return</param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A <see cref="ScoresResponse"/></returns>
    [HttpPost("{userId:int}/scores")]
    [AllowAnonymous]
    public async Task<ActionResult<ScoresResponse>> GetUserScoresAsync(
        int userId,
        [FromBody] ScoreQueryCommand command,
        [FromQuery] int? page,
        [FromQuery] int? amount,
        CancellationToken ct = default)
    {
        var scoresPage = page ?? 1;
        var scoresAmount = amount ?? 25;

        if (scoresAmount > 100 || scoresAmount < 1) return BadRequest($"{nameof(scoresAmount)} must be between 1 and 100");
        if (command.IncludeMods.Intersect(command.ExcludeMods).Any()) 
            return BadRequest($"{nameof(command.IncludeMods)} must not contain any mods from {nameof(command.ExcludeMods)}");
        if (command.BeatmapStatus != null && !_eligibleStatuses.Contains(command.BeatmapStatus.Value))
        {
            return BadRequest($"{nameof(command.BeatmapStatus)} must be ranked, approved or loved");
        }
        
        var user = await userRepository.GetByIdWithCountryAsync(userId, ct);
        if (user == null) return NotFound("User not found");
        
        var filteredCommand = new ScoreQueryCommand
        {
            Modes = command.Modes,
            DateRange = command.DateRange,
            RankRange = command.RankRange,
            PpRange = command.PpRange,
            AccuracyRange = command.AccuracyRange,
            SpeedRange = command.SpeedRange,
            SortBy = command.SortBy,
            IsDescending = command.IsDescending,
            ExcludeConverts = command.ExcludeConverts,
            BeatmapStatus = command.BeatmapStatus
        };

        var clampedCommand = FilterUtils.GetCommandWithClampedRanges(filteredCommand);
        var commandHash = ResponseCachingUtils.GetScoreCommandHashForCache(clampedCommand);
        var scoresQueryString = ResponseCachingUtils.GetScoreQueryKeyForCache(commandHash, amount, page);
        var scoresResponseString = $"userscores-{userId}-{scoresQueryString}";
        
        var scoresResponseCache = await scoreCacheRepository.GetSerializedResponseAsync(scoresResponseString);

        if (scoresResponseCache is null)
        {
            var query = FilterUtils.FilterScoreQuery(scoreRepository.GetByUserId(userId), clampedCommand);
            
            scoresQueryString = ResponseCachingUtils.GetScoreQueryKeyForCache(commandHash, amount, scoresPage);
            scoresResponseString = $"userscores-{userId}-{scoresQueryString}";
            
            query = query.Skip(scoresAmount * (scoresPage - 1)).Take(scoresAmount + 1);
            var scores = await query.ToListAsync(ct);

            var moreAvailable = scores.Count > scoresAmount;
            scores = scores.Take(scoresAmount).ToList();
            
            var beatmaps = await beatmapRepository
                .GetBulkWithBeatmapsetsAsync(scores.Select(s => s.BeatmapId).Distinct().ToList(), ct);

            scores = scores.Select(s =>
            {
                s.Beatmap = beatmaps.First(b => b.Id == s.BeatmapId);
                s.User = user;
                return s;
            }).ToList();
            
            var apiScores = scores.Select(DbToApiObjectUtils.DbScoreToApiScore).ToList();
            
            var scoresResponse = new ScoresResponse
            {
                Scores = apiScores,
                MoreAvailable = moreAvailable
            };

            var serializedString = ResponseCachingUtils.GetSerializedScoresResponse(scoresResponse);
            await scoreCacheRepository.SetSerializedResponseAsync(scoresResponseString, serializedString, _scoresTtl);
            
            return Ok(scoresResponse);
        }
        else
        {
            var scoresResponse = ResponseCachingUtils.GetDeserializedScoresResponse(scoresResponseCache);
            return Ok(scoresResponse);
        }
    }

    /// <summary>
    /// Get user's statistics
    /// </summary>
    /// <param name="userId">The <see cref="User"/> ID</param>
    /// <param name="command">The <see cref="ScoreQueryCommand"/></param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>The statistics.</returns>
    [HttpPost("{userId:int}/stats")]
    [AllowAnonymous]
    public async Task<ActionResult<UserStatsResponse>> GetUserStats(int userId, [FromBody] ScoreQueryCommand command,
        CancellationToken ct = default)
    {
        if (command.IncludeMods.Intersect(command.ExcludeMods).Any()) 
            return BadRequest($"{nameof(command.IncludeMods)} must not contain any mods from {nameof(command.ExcludeMods)}");
        if (command.BeatmapStatus != null && !_eligibleStatuses.Contains(command.BeatmapStatus.Value))
        {
            return BadRequest($"{nameof(command.BeatmapStatus)} must be ranked, approved or loved");
        }
        
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user == null) return NotFound("User not found");
        
        var filteredCommand = new ScoreQueryCommand
        {
            Modes = command.Modes,
            DateRange = command.DateRange,
            RankRange = command.RankRange,
            PpRange = command.PpRange,
            AccuracyRange = command.AccuracyRange,
            SpeedRange = command.SpeedRange,
            ExcludeConverts = command.ExcludeConverts,
            BeatmapStatus = command.BeatmapStatus
        };
        
        var clampedCommand = FilterUtils.GetCommandWithClampedRanges(filteredCommand);
        var commandHash = ResponseCachingUtils.GetScoreCommandHashForCache(clampedCommand);
        var cacheKey = ResponseCachingUtils.GetUserStatsKeyForCache(commandHash, userId);

        var userStatsCache = await scoreCacheRepository.GetSerializedResponseAsync(cacheKey);

        if (userStatsCache is null)
        {
            var query = FilterUtils.FilterScoreQuery(scoreRepository.GetByUserId(userId), clampedCommand);
            var relevantData = await query
                .AsNoTracking()
                .Select(s => new { s.Date, s.Rank, s.SpeedChange, s.Beatmap!.Difficulty })
                .ToListAsync(ct);
            
            // The score counts history
            var countsByMonth = relevantData.Select(d => d.Date)
                .OrderBy(d => d)
                .GroupBy(d => new DateTime(d.Year, d.Month, 1))
                .Select(g => new UserHistory
                {
                    Month = g.Key,
                    MonthlyCount = g.Count(),
                })
                .ToList();

            var accumulatedCount = 0;
            var history = countsByMonth.Select(h =>
            {
                accumulatedCount += h.MonthlyCount;
                return new UserHistory { Month = h.Month, MonthlyCount = accumulatedCount };
            })
            .ToList();
            
            // The rank distribution
            var ranks = relevantData.Select(d => d.Rank).ToList();
            var ranksDict = new Dictionary<int, int>
            {
                {1, ranks.Count(r => r <= 1)},
                {3, ranks.Count(r => r <= 3)},
                {8, ranks.Count(r => r <= 8)},
                {25, ranks.Count(r => r <= 25)},
                {50, ranks.Count(r => r <= 50)},
                {100, ranks.Count(r => r <= 100)}
            };
            var rankStats = ranksDict.Select(kvp => new RankStats
            {
                RankBound = kvp.Key,
                Count = kvp.Value,
            })
            .ToList();
            
            // The speed distribution
            var speedStats = relevantData
                .Select(d => d.SpeedChange)
                .Where(s => s != null)
                .OrderBy(s => s)
                .GroupBy(s => s < 1 
                    ? Math.Floor((double)s * 20) / 20
                    : Math.Floor((double)s! * 10) / 10)
                .Select(g => new UserSpeedStats
                {
                    SpeedBracket = g.Key,
                    Count = g.Count()
                })
                .ToList();
            
            // The star distribution
            var starStats = relevantData
                .Select(d => d.Difficulty)
                .OrderBy(s => s)
                .GroupBy(s => s > 10 ? 10 : Math.Floor(s))
                .Select(g => new UserStars
                {
                    SrBracket = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var statsResponse = new UserStatsResponse
            {
                History = history,
                RankDistribution = rankStats,
                StarsDistribution = starStats,
                SpeedDistribution = speedStats
            };
            
            var serializedString = ResponseCachingUtils.GetSerializedUserStatsResponse(statsResponse);
            await scoreCacheRepository.SetSerializedResponseAsync(cacheKey, serializedString, _statsTtl);

            return Ok(statsResponse);
        }
        else
        {
            var statsResponse = ResponseCachingUtils.GetDeserializedUserStatsResponse(userStatsCache);
            return Ok(statsResponse);
        }
    }

    /// <summary>
    /// Search users
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>A List of <see cref="User"/>s</returns>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ApiUser>>> SearchUsers([FromQuery] string query, CancellationToken ct = default)
    {
        var users = await userRepository.SearchAsync(query, ct);
        var apiUsers = users.Select(DbToApiObjectUtils.DbUserToApiUser).ToList();
        return Ok(apiUsers);
    }
}