using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.ExternalAPIs.Services.OsuApi;
using Microsoft.AspNetCore.Mvc;
using Lazerboard.Data.OsuEntities.OsuApiEntities;

namespace Lazerboard.ExternalAPIs.Apis;

[ApiController]
[Route("osuapi")]
public class OsuApiController(OsuApiService osuApiService, ILogger<OsuApiController> logger) : ControllerBase
{
    /// <summary>
    /// Get beatmapsets from the API beatmapsets search endpoint (sorted by date ranked, ascending)
    /// </summary>
    /// <param name="cursor">Cursor string</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated <see cref="BeatmapsetsResponse"/> object</returns>
    [HttpGet("beatmapsets")]
    public async Task<IActionResult> GetBeatmapsetsAsync([FromQuery] string? cursor, CancellationToken ct = default)
    {
        var beatmapsetsResponse = await osuApiService.GetBeatmapsetsAsync(cursor, ct);
        return Ok(beatmapsetsResponse);
    }

    /// <summary>
    /// Get beatmapset data from the API
    /// </summary>
    /// <param name="id">The <see cref="APIBeatmapset"/> ID</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="APIBeatmapset"/></returns>
    [HttpGet("beatmapsets/{id:int}")]
    public async Task<IActionResult> GetBeatmapsetAsync(int id, CancellationToken ct = default)
    {
        var beatmapset = await osuApiService.GetBeatmapsetAsync(id, ct);
        return Ok(beatmapset);
    }

    /// <summary>
    /// Get beatmap scores from the API
    /// </summary>
    /// <param name="beatmapId">Beatmap ID</param>
    /// <param name="mode">Ruleset (osu, taiko, fruits, mania)</param>
    /// <param name="legacyOnly">Whether to exclude lazer scores or not (0 = include, 1 = exclude)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated BeatmapScores object</returns>
    [HttpGet("beatmaps/{id:int}/scores")]
    public async Task<IActionResult> GetBeatmapScoresAsync(int beatmapId, 
        [FromQuery] Mode? mode, 
        [FromQuery] int legacyOnly = 0,
        CancellationToken ct = default)
    {
        var scores = await osuApiService.GetBeatmapScoresAsync(beatmapId, mode, legacyOnly, ct);
        return Ok(scores);
    }

    /// <summary>
    /// Get scores from the API firehose
    /// </summary>
    /// <param name="cursor">Cursor string (used to fetch new scores since last call)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated ScoresResponse object with the cursor string and array of Scores</returns>
    [HttpGet("scores")]
    public async Task<IActionResult> GetScoresAsync([FromQuery] string? cursor, CancellationToken ct = default)
    {
        var scoresResponse = await osuApiService.GetScoresAsync(cursor, ct);
        return Ok(scoresResponse);
    }

    /// <summary>
    /// Get API Beatmap data from their IDs
    /// </summary>
    /// <param name="ids">Array containing beatmap IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Array with populated APIBeatmap objects</returns>
    public async Task<IActionResult> GetBeatmapsAsync([FromQuery] int[] ids, CancellationToken ct = default)
    {
        var beatmaps = await osuApiService.GetBeatmapsAsync(ids, ct);
        return Ok(beatmaps);
    }

    /// <summary>
    /// Get API User data from their IDs
    /// </summary>
    /// <param name="ids">List containing user IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Array with populated APIUser objects</returns>
    public async Task<IActionResult> GetUsersAsync([FromQuery] int[] ids, CancellationToken ct = default)
    {
        var users = await osuApiService.GetUsersAsync(ids, ct);
        return Ok(users);
    }

    /// <summary>
    /// Download a map from the API
    /// </summary>
    /// <param name="id">The beatmap ID</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("beatmaps/{id:int}/download")]
    public async Task<IActionResult> DownloadBeatmapAsync(int id, CancellationToken ct = default)
    {
        var stream = await osuApiService.DownloadBeatmapAsync(id, ct);
        return File(stream, "octet-stream", $"{id}.osu");
    }
}