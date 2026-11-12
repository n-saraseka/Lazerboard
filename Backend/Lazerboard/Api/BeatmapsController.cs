using Lazerboard.Api.Dtos;
using Lazerboard.Api.Utils;
using Lazerboard.Data.Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;

namespace Lazerboard.Api;

[ApiController]
[Route("api/[controller]")]
public class BeatmapsController(IBeatmapRepository beatmapRepository,
    IScoreRepository scoreRepository) : ControllerBase
{
    /// <summary>
    /// Get beatmap scores
    /// </summary>
    /// <param name="id">The <see cref="Beatmap"/> ID</param>
    /// <param name="mode">The <see cref="Mode"/> </param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A <see cref="ScoresResponse"/></returns>
    [HttpGet("{id:int}/scores")]
    [AllowAnonymous]
    public async Task<ActionResult<ScoresResponse>> GetBeatmapScoresAsync(int id, 
        [FromQuery] Mode mode,
        CancellationToken ct = default)
    {
        
        var beatmap = await beatmapRepository.GetByIdAsync(id, ct);
        if (beatmap == null) return NotFound("Beatmap not found");

        var count = await scoreRepository.GetBeatmapScoreCount(id, mode, ct);

        var scores = await scoreRepository.GetByBeatmapIdWithUserDataAsync(id, mode, ct);
        var apiScores = scores.Select(DbToApiObjectUtils.DbScoreToApiScore).ToList();
        
        return Ok(new ScoresResponse
        {
            Scores = apiScores,
            Count = count
        });
    }
        
        
}