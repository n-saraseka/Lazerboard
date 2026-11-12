using Lazerboard.Api.Dtos;
using Lazerboard.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lazerboard.Data.Database.Entities;
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

        if (!user.IsRegistered)
        {
            user.IsRegistered = true;
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
    public async Task<ActionResult<UserScoresResponse>> GetUserScoresAsync(
        int userId,
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
            IncludeMods = command.IncludeMods,
            ExcludeMods = command.ExcludeMods,
            LenientMode = command.LenientMode,
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
            var query = scoreRepository.GetByUserId(userId);
            var count = await query.CountAsync(ct);
            
            query = FilterUtils.FilterScoreQuery(query, clampedCommand);
            var filteredCount = await query.CountAsync(ct);
            
            var pages = (int)Math.Ceiling((double)count / scoresAmount);
            if (scoresPage > pages) scoresPage = Math.Max(pages, 1);
            
            scoresQueryString = ResponseCachingUtils.GetScoreQueryKeyForCache(commandHash, amount, scoresPage);
            scoresResponseString = $"userscores-{userId}-{scoresQueryString}";
            
            query = query
                .Skip(scoresAmount * (scoresPage - 1)).Take(scoresAmount);
            var scores = await query.ToListAsync(ct);
            
            var beatmaps = await beatmapRepository
                .GetBulkWithBeatmapsetsAsync(scores.Select(s => s.BeatmapId).Distinct().ToList(), ct);

            scores = scores.Select(s =>
            {
                s.Beatmap = beatmaps.First(b => b.Id == s.BeatmapId);
                s.User = user;
                return s;
            }).ToList();
            
            var apiScores = scores.Select(DbToApiObjectUtils.DbScoreToApiScore).ToList();
            
            var scoresResponse = new UserScoresResponse
            {
                Scores = apiScores,
                Count = count,
                FilteredCount = filteredCount
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
            var scoresResponse = ResponseCachingUtils.GetDeserializedScoresResponse(scoresResponseCache, true);
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
            IncludeMods = command.IncludeMods,
            ExcludeMods = command.ExcludeMods,
            LenientMode = command.LenientMode,
            ExcludeConverts = command.ExcludeConverts,
            BeatmapStatus = command.BeatmapStatus
        };
        
        var clampedCommand = FilterUtils.GetCommandWithClampedRanges(filteredCommand);
        var commandHash = ResponseCachingUtils.GetScoreCommandHashForCache(clampedCommand);
        var cacheKey = ResponseCachingUtils.GetUserStatsKeyForCache(commandHash, userId);

        var userStatsCache = await scoreCacheRepository.GetSerializedResponseAsync(cacheKey);

        if (userStatsCache is null)
        {
            var filterData = FilterUtils.GetFilterText(clampedCommand);
            var filterText = filterData.FilterText.Length == 0 ? "" : $" AND {filterData.FilterText}";
            var parameters = filterData.Parameters;
            var tableString = FilterUtils.GetTableStringWithJoins(clampedCommand, false);
            
            var context = scoreRepository.GetDbContext();
            
            // The score counts history
            var historySql = $@"SELECT month, SUM(count) OVER (ORDER BY month ASC ROWS UNBOUNDED PRECEDING) monthly_count
                        FROM (SELECT DATE_TRUNC('month', date) as month, COUNT(*) as count FROM {tableString}
                        WHERE user_id = {userId}{filterText}
                        GROUP BY month
                        ORDER BY month ASC)";
            var historyQuery = context.Database.SqlQueryRaw<UserHistory>(historySql, parameters);
            var history = await historyQuery.ToListAsync(ct);
            
            // The rank distribution
            var ranksSql = $@"SELECT i.rank_bound,
                        CASE i.rank_bound
                            WHEN 1   THEN top1  WHEN 3   THEN top3  WHEN 8   THEN top8
                            WHEN 25  THEN top25 WHEN 50  THEN top50 WHEN 100 THEN top100
                        END AS count
                        FROM (
                            SELECT
                            COUNT(*) FILTER (WHERE s.rank <= 1)   AS top1,
                            COUNT(*) FILTER (WHERE s.rank <= 3)   AS top3,
                            COUNT(*) FILTER (WHERE s.rank <= 8)   AS top8,
                            COUNT(*) FILTER (WHERE s.rank <= 25)  AS top25,
                            COUNT(*) FILTER (WHERE s.rank <= 50)  AS top50,
                            COUNT(*) FILTER (WHERE s.rank <= 100) AS top100
                        FROM {tableString}
                        WHERE user_id = {userId}{filterText}
                        ) rank_distribution
                        CROSS JOIN (VALUES (1),(3),(8),(25),(50),(100)) i(rank_bound)
                        ORDER BY i.rank_bound";
            var ranksQuery = context.Database.SqlQueryRaw<RankStats>(ranksSql, parameters);
            var rankStats = await ranksQuery.ToListAsync(ct);
            
            // The star distribution
            tableString = FilterUtils.GetTableStringWithJoins(clampedCommand, true);
            var starSql = $@"SELECT CASE WHEN b.difficulty >= 10 THEN 10 ELSE FLOOR(b.difficulty) END AS sr_bracket, COUNT(*) AS count
                        FROM {tableString}
                        WHERE user_id = {userId}{filterText}
                        GROUP BY sr_bracket
                        ORDER BY sr_bracket";
            var starsQuery = context.Database.SqlQueryRaw<UserStars>(starSql, parameters);
            var starStats = await starsQuery.ToListAsync(ct);
            
            // The speed distribution
            tableString = FilterUtils.GetTableStringWithJoins(clampedCommand, false);
            var speedSql = $@"SELECT CASE WHEN speed_change < 1
                        THEN FLOOR(speed_change * 20) / 20
                        ELSE FLOOR(speed_change * 10) / 10 END AS speed_bracket, COUNT(*) AS count
                        FROM {tableString}
                        WHERE user_id = {userId} AND speed_change IS NOT NULL{filterText}
                        GROUP BY speed_bracket
                        ORDER BY speed_bracket";
            var speedQuery = context.Database.SqlQueryRaw<UserSpeedStats>(speedSql, parameters);
            var speedStats = await speedQuery.ToListAsync(ct);

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