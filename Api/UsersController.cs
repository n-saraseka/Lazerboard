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
    public async Task<IActionResult> GetUserScoresAsync(
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
        var scoresPage = page ?? 1;
        var scoresAmount = amount ?? 25;

        if (scoresAmount > 100) return BadRequest($"{nameof(scoresAmount)} must be less or equal to 100");
        
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user == null) return NotFound("User not found");
        
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
        var pages = (int)Math.Ceiling((double)count / scoresAmount);
        if (scoresPage > pages) scoresPage = Math.Max(pages, 1);
        
        query = query
            .Skip(scoresAmount * (scoresPage - 1)).Take(scoresAmount);

        var scores = await query.ToListAsync(ct);

        return Ok(new ScoresResponse
        {
            Scores = scores,
            Count = count,
        });
    }

    /// <summary>
    /// Get user data
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="mode">A <see cref="Mode"/> to get specific data from (returns data from all modes if not provided)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A <see cref="UserDataResponse"/></returns>
    [HttpGet("{userId:int}/data")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserDataAsync(int userId, [FromQuery] Mode? mode, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user == null) return NotFound("User not found");
        
        var context = scoreRepository.GetDbContext();

        var historyQuery = mode == null
            ? context.Database.SqlQuery<UserHistory>(
                $"SELECT month, SUM(count) OVER (ORDER BY month ASC ROWS UNBOUNDED PRECEDING) monthly_count\nFROM (SELECT DATE_TRUNC('month', date) as month, COUNT(*) as count FROM scores\nWHERE user_id = {userId} AND rank<=100\nGROUP BY month\nORDER BY month ASC)")
            : context.Database.SqlQuery<UserHistory>(
                $"SELECT month, SUM(count) OVER (ORDER BY month ASC ROWS UNBOUNDED PRECEDING) monthly_count\nFROM (SELECT DATE_TRUNC('month', date) as month, COUNT(*) as count FROM scores\nWHERE user_id = {userId} AND rank<=100 AND mode={mode}\nGROUP BY month\nORDER BY month ASC)");
        
        var history = await historyQuery.ToListAsync(ct);

        var starsQuery = mode == null
            ? context.Database.SqlQuery<UserStars>(
                $"SELECT CASE WHEN b.difficulty >= 10 THEN 10 ELSE FLOOR(b.difficulty) END AS sr_bracket, COUNT(*) AS count\nFROM scores s INNER JOIN beatmaps b ON s.beatmap_id = b.id \nWHERE user_id = {userId}\nGROUP BY sr_bracket\nORDER BY sr_bracket")
            : context.Database.SqlQuery<UserStars>(
                $"SELECT CASE WHEN b.difficulty >= 10 THEN 10 ELSE FLOOR(b.difficulty) END AS sr_bracket, COUNT(*) AS count\nFROM scores s INNER JOIN beatmaps b ON s.beatmap_id = b.id \nWHERE user_id = {userId} AND s.mode={mode}\nGROUP BY sr_bracket\nORDER BY sr_bracket");
        
        var starStats = await starsQuery.ToListAsync(ct);

        var ranksQuery = mode == null
            ? context.Database.SqlQuery<RankStats>(
                $"WITH intervals AS(\n\tSELECT 1 AS rank_bound UNION ALL\n\tSELECT 5 UNION ALL\n\tSELECT 10 UNION ALL\n\tSELECT 25 UNION ALL\n\tSELECT 50 UNION ALL\n\tSELECT 100\n),\nscore_data AS (\nSELECT * FROM scores\nWHERE user_id = {userId})\nSELECT i.rank_bound, COUNT(*) as count\nFROM score_data s JOIN intervals i ON s.rank <= i.rank_bound\nGROUP BY i.rank_bound\nORDER BY i.rank_bound")
            : context.Database.SqlQuery<RankStats>(
                $"WITH intervals AS(\n\tSELECT 1 AS rank_bound UNION ALL\n\tSELECT 5 UNION ALL\n\tSELECT 10 UNION ALL\n\tSELECT 25 UNION ALL\n\tSELECT 50 UNION ALL\n\tSELECT 100\n),\nscore_data AS (\nSELECT * FROM scores\nWHERE user_id = {userId} AND mode={mode})\nSELECT i.rank_bound, COUNT(*) as count\nFROM score_data s JOIN intervals i ON s.rank <= i.rank_bound\nGROUP BY i.rank_bound\nORDER BY i.rank_bound");
        
        var rankStats = await ranksQuery.ToListAsync(ct);

        var speedQuery = mode == null
            ? context.Database.SqlQuery<UserSpeedStats>(
                $"SELECT CASE WHEN speed_change < 1 \nTHEN FLOOR(speed_change * 20) / 20 \nELSE FLOOR(speed_change * 10) / 10 END AS speed_bracket, COUNT(*) AS count\nFROM scores\nWHERE user_id = {userId} AND speed_change IS NOT NULL\nGROUP by speed_bracket\nORDER BY speed_bracket")
            : context.Database.SqlQuery<UserSpeedStats>(
                $"SELECT CASE WHEN speed_change < 1 \nTHEN FLOOR(speed_change * 20) / 20 \nELSE FLOOR(speed_change * 10) / 10 END AS speed_bracket, COUNT(*) AS count\nFROM scores \nWHERE user_id = {userId} AND mode = {mode} AND speed_change IS NOT NULL\nGROUP by speed_bracket\nORDER BY speed_bracket");

        var speedStats = await speedQuery.ToListAsync(ct);

        var query = scoreRepository.GetAll().Where(s => s.UserId == userId);
        if (mode != null) query = query.Where(s => s.Mode == mode);
        var count = await query.CountAsync(ct);

        return Ok(new UserDataResponse 
        {
            Count = count,
            History = history,
            StarStats = starStats,
            RankStats = rankStats,
            SpeedStats = speedStats
        });
    }

    public async Task<List<User>> SearchUsers(string query, CancellationToken ct = default) =>
        await userRepository.SearchAsync(query, ct);
}