using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OsuScoreStats.Api.Dtos;
using OsuScoreStats.Shared.DbService.Repositories.Interfaces;
using OsuScoreStats.Shared.OsuApi.Enums;

namespace OsuScoreStats.Api;

[ApiController]
[Route("api/[controller]")]
public class BeatmapsController(IBeatmapRepository beatmapRepository,
    IScoreRepository scoreRepository) : ControllerBase
{
    /// <summary>
    /// Get collected scores on the <see cref="Beatmap"/> from the API
    /// </summary>
    /// <remarks>Returns up to 100 scores per page</remarks>
    /// <param name="id">The <see cref="Beatmap"/> ID</param>
    /// <param name="mode">The <see cref="Mode"/> </param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A <see cref="ScoresResponse"/></returns>
    [HttpGet("{id:int}/scores")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBeatmapScoresAsync(int id, 
        [FromQuery] Mode mode,
        [FromQuery] int? page,
        CancellationToken ct = default)
    {
        var mapPage = page ?? 1;
        
        var beatmap = await beatmapRepository.GetByIdAsync(id, ct);
        if (beatmap == null) return NotFound("Beatmap not found");

        var count = await scoreRepository.GetBeatmapScoreCount(id, mode, ct);
        var pages = (int)Math.Ceiling(count / 100d);
        if (page > pages) mapPage = Math.Max(1, pages);

        var scores = await scoreRepository.GetByBeatmapIdWithUserDataAsync(id, mode, mapPage, ct);
        
        return Ok(new ScoresResponse
        {
            Scores = scores,
            Count = count
        });
    }
        
        
}