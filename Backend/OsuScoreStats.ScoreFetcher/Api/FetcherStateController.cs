using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OsuScoreStats.ScoreFetcher.Processing;

namespace OsuScoreStats.ScoreFetcher.Api;

[ApiController]
[Route("api/[controller]")]
public class FetcherStateController(ISeedingState seedingState) : ControllerBase
{
    /// <summary>
    /// Get the fetcher state.
    /// </summary>
    /// <returns>True if seeding, false if seeding is complete</returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetSeedingState()
    {
        return Ok(seedingState.IsSeeding);
    }
}