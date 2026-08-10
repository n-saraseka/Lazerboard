using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsuScoreStats.Api.Dtos;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.OsuApi.Enums;

namespace OsuScoreStats.Api;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IScoreRepository scoreRepository) : ControllerBase
{
    /// <summary>
    /// Get user scores
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="modes">Gameplay modes to get scores from (Osu, Taiko, Fruits, Mania)</param>
    /// <param name="rankMin">Minimum map rank threshold</param>
    /// <param name="rankMax">Maximum map rank threshold</param>
    /// <param name="ppMin">Minimum PP threshold</param>
    /// <param name="ppMax">Maximum PP threshold</param>
    /// <param name="accMin">Minimum accuracy threshold</param>
    /// <param name="accMax">Maximum accuracy threshold</param>
    /// <param name="speedMin">Minimum speed threshold</param>
    /// <param name="speedMax">Maximum speed threshold</param>
    /// <param name="mods">Mods to count scores with</param>
    /// <param name="lenientMode">Whether to allow other mods than <paramref name="mods"/></param>
    /// <param name="dateMin">Date to begin getting scores from (defaults to Unix epoch)</param>
    /// <param name="dateMax">Date to end getting scores from (defaults to latest date in scores table)</param>
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
        [FromQuery] int? rankMin,
        [FromQuery] int? rankMax,
        [FromQuery] int? ppMin,
        [FromQuery] int? ppMax,
        [FromQuery] double? accMin,
        [FromQuery] double? accMax,
        [FromQuery] double? speedMin,
        [FromQuery] double? speedMax,
        [FromQuery] string[] mods,
        [FromQuery] bool lenientMode,
        [FromQuery] DateOnly? dateMin,
        [FromQuery] DateOnly? dateMax,
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
        var targetStartDate = dateMin ?? DateOnly.FromDateTime(DateTime.UnixEpoch);
        var targetEndDate = dateMax ?? DateOnly.FromDateTime(latestDate);
        
        query = FilterUtils.FilterScoreQuery(query, 
            modes,
            [targetStartDate, targetEndDate],
            [rankMin, rankMax],
            [ppMin, ppMax],
            [accMin, accMax],
            [speedMin, speedMax],
            [],
            mods,
            lenientMode,
            null,
            sort,
            isDesc);
        
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

    /// <summary>
    /// Get user data
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A <see cref="UserDataResponse"/></returns>
    [HttpGet("{userId:int}/data")]
    [AllowAnonymous]
    public async Task<UserDataResponse> GetUserDataAsync(int userId, CancellationToken ct = default)
    {
        var context = scoreRepository.GetDbContext();

        var history = await context.Database.SqlQuery<UserHistory>(
                $"SELECT month, SUM(count) OVER (ORDER BY month ASC ROWS UNBOUNDED PRECEDING) monthly_count\nFROM (SELECT DATE_TRUNC('month', date) as month, COUNT(*) as count FROM scores\nWHERE user_id = {userId}\nGROUP BY month\nORDER BY month ASC)")
            .ToListAsync(ct);
        
        var starStats = await context.Database.SqlQuery<UserStars>(
                $"SELECT CASE WHEN b.difficulty >= 10 THEN 10 ELSE FLOOR(b.difficulty) END AS sr_bracket, COUNT(*) AS count\nFROM scores s INNER JOIN beatmaps b ON s.beatmap_id = b.id \nWHERE user_id = {userId}\nGROUP BY sr_bracket\nORDER BY sr_bracket")
            .ToListAsync(ct);
        
        var rankStats = await context.Database.SqlQuery<RankStats>(
                $"WITH intervals AS(\n\tSELECT 1 AS rank_bound UNION ALL\n\tSELECT 5 UNION ALL\n\tSELECT 10 UNION ALL\n\tSELECT 25 UNION ALL\n\tSELECT 50 UNION ALL\n\tSELECT 100\n),\nscore_data AS (\nSELECT * FROM scores\nWHERE user_id = {userId})\nSELECT i.rank_bound, COUNT(*) as count\nFROM score_data s JOIN intervals i ON s.rank <= i.rank_bound\nGROUP BY i.rank_bound\nORDER BY i.rank_bound")
            .ToListAsync(ct);

        var query = scoreRepository.GetAll().Where(s => s.UserId == userId);
        var count = await query.CountAsync(ct);

        return new UserDataResponse
        {
            Count = count,
            History = history,
            StarStats = starStats,
            RankStats = rankStats
        };
    }
}