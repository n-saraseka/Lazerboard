using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.OsuApi.Enums;

namespace OsuScoreStats.Api;

[ApiController]
[Route("api/[controller]")]
public class BeatmapsController(IBeatmapRepository beatmapRepository, 
    IBeatmapsetRepository beatmapsetRepository, 
    IScoreRepository scoreRepository) : ControllerBase
{
    /// <summary>
    /// Get collected scores on the <see cref="Beatmap"/> from the API
    /// </summary>
    /// <param name="id">The <see cref="Beatmap"/> ID</param>
    /// <param name="mode">The <see cref="Mode"/> </param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A List of <see cref="Score"/>s</returns>
    [HttpGet("{id:int}/scores")]
    [AllowAnonymous]
    public async Task<List<Score>> GetBeatmapScoresAsync(int id, Mode mode, CancellationToken ct = default) => 
        await scoreRepository.GetByBeatmapIdWithUserDataAsync(id, mode, ct);
        
}