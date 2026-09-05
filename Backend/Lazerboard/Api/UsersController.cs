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
public class UsersController(IScoreRepository scoreRepository, IUserRepository userRepository, IBeatmapRepository beatmapRepository) : ControllerBase
{
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
    public async Task<IActionResult> GetUserScoresAsync(
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
        
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user == null) return NotFound("User not found");
        
        var query = scoreRepository.GetAll().Where(s => s.UserId == userId);
        
        var latestDate = await query.MaxAsync(s => s.Date, ct);
        var targetStartDate = command.DateRange[0] ?? DateOnly.FromDateTime(DateTime.UnixEpoch);
        var targetEndDate = command.DateRange[1] ?? DateOnly.FromDateTime(latestDate);
        
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
            SortBy = command.SortBy,
            IsDescending = command.IsDescending
        };
        
        query = FilterUtils.FilterScoreQuery(query, filteredCommand);
        
        var count = await query.CountAsync(ct);
        var pages = (int)Math.Ceiling((double)count / scoresAmount);
        if (scoresPage > pages) scoresPage = Math.Max(pages, 1);
        
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

        return Ok(new ScoresResponse
        {
            Scores = scores,
            Count = count,
        });
    }

    /// <summary>
    /// Get <see cref="User"/>'s top 100 scores count history
    /// </summary>
    /// <param name="userId">The <see cref="User"/> ID</param>
    /// <param name="mode">The specified <see cref="Mode"/></param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="UserHistory"/></returns>
    [HttpGet("{userId:int}/history")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserHistory(int userId, [FromQuery] Mode? mode, CancellationToken ct = default)
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
        
        return Ok(new { History = history });
    }
    
    /// <summary>
    /// Get <see cref="User"/>'s star rating distribution across their saved scores
    /// </summary>
    /// <param name="userId">The <see cref="User"/> ID</param>
    /// <param name="mode">The specified <see cref="Mode"/></param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="UserStars"/></returns>
    [HttpGet("{userId:int}/stardistribution")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserStarDistribution(int userId, [FromQuery] Mode? mode, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user == null) return NotFound("User not found");
        
        var context = scoreRepository.GetDbContext();
        
        var starsQuery = mode == null
            ? context.Database.SqlQuery<UserStars>(
                $"SELECT CASE WHEN b.difficulty >= 10 THEN 10 ELSE FLOOR(b.difficulty) END AS sr_bracket, COUNT(*) AS count\nFROM scores s INNER JOIN beatmaps b ON s.beatmap_id = b.id \nWHERE user_id = {userId}\nGROUP BY sr_bracket\nORDER BY sr_bracket")
            : context.Database.SqlQuery<UserStars>(
                $"SELECT CASE WHEN b.difficulty >= 10 THEN 10 ELSE FLOOR(b.difficulty) END AS sr_bracket, COUNT(*) AS count\nFROM scores s INNER JOIN beatmaps b ON s.beatmap_id = b.id \nWHERE user_id = {userId} AND s.mode={mode}\nGROUP BY sr_bracket\nORDER BY sr_bracket");
        
        var starStats = await starsQuery.ToListAsync(ct);
        
        return Ok(new { StarStats = starStats });
    }
    
    /// <summary>
    /// Get <see cref="User"/>'s top 100 rank distribution across their saved scores
    /// </summary>
    /// <param name="userId">The <see cref="User"/> ID</param>
    /// <param name="mode">The specified <see cref="Mode"/></param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="RankStats"/></returns>
    [HttpGet("{userId:int}/rankdistribution")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserRankDistribution(int userId, [FromQuery] Mode? mode, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user == null) return NotFound("User not found");
        
        var context = scoreRepository.GetDbContext();
        
        var ranksQuery = mode == null
            ? context.Database.SqlQuery<RankStats>(
                $"WITH intervals AS(\n\tSELECT 1 AS rank_bound UNION ALL\n\tSELECT 5 UNION ALL\n\tSELECT 10 UNION ALL\n\tSELECT 25 UNION ALL\n\tSELECT 50 UNION ALL\n\tSELECT 100\n),\nscore_data AS (\nSELECT * FROM scores\nWHERE user_id = {userId})\nSELECT i.rank_bound, COUNT(*) as count\nFROM score_data s JOIN intervals i ON s.rank <= i.rank_bound\nGROUP BY i.rank_bound\nORDER BY i.rank_bound")
            : context.Database.SqlQuery<RankStats>(
                $"WITH intervals AS(\n\tSELECT 1 AS rank_bound UNION ALL\n\tSELECT 5 UNION ALL\n\tSELECT 10 UNION ALL\n\tSELECT 25 UNION ALL\n\tSELECT 50 UNION ALL\n\tSELECT 100\n),\nscore_data AS (\nSELECT * FROM scores\nWHERE user_id = {userId} AND mode={mode})\nSELECT i.rank_bound, COUNT(*) as count\nFROM score_data s JOIN intervals i ON s.rank <= i.rank_bound\nGROUP BY i.rank_bound\nORDER BY i.rank_bound");
        
        var rankStats = await ranksQuery.ToListAsync(ct);
        
        return Ok(new { RankStats = rankStats });
    }
    
    /// <summary>
    /// Get <see cref="User"/>'s speed change distribution across their saved scores
    /// </summary>
    /// <param name="userId">The <see cref="User"/> ID</param>
    /// <param name="mode">The specified <see cref="Mode"/></param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="UserSpeedStats"/></returns>
    [HttpGet("{userId:int}/speeddistribution")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserSpeedDistribution(int userId, [FromQuery] Mode? mode, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user == null) return NotFound("User not found");
        
        var context = scoreRepository.GetDbContext();
        
        var speedQuery = mode == null
            ? context.Database.SqlQuery<UserSpeedStats>(
                $"SELECT CASE WHEN speed_change < 1 \nTHEN FLOOR(speed_change * 20) / 20 \nELSE FLOOR(speed_change * 10) / 10 END AS speed_bracket, COUNT(*) AS count\nFROM scores\nWHERE user_id = {userId} AND speed_change IS NOT NULL\nGROUP by speed_bracket\nORDER BY speed_bracket")
            : context.Database.SqlQuery<UserSpeedStats>(
                $"SELECT CASE WHEN speed_change < 1 \nTHEN FLOOR(speed_change * 20) / 20 \nELSE FLOOR(speed_change * 10) / 10 END AS speed_bracket, COUNT(*) AS count\nFROM scores \nWHERE user_id = {userId} AND mode = {mode} AND speed_change IS NOT NULL\nGROUP by speed_bracket\nORDER BY speed_bracket");

        var speedStats = await speedQuery.ToListAsync(ct);
        
        return Ok(new { SpeedStats = speedStats });
    }

    /// <summary>
    /// Search <see cref="User"/>s
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>A List of <see cref="User"/>s</returns>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<List<User>> SearchUsers([FromQuery] string query, CancellationToken ct = default) =>
        await userRepository.SearchAsync(query, ct);
}