using Lazerboard.ScoreFetcher.Processing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lazerboard.ScoreFetcher.Api;

[ApiController]
[Route("api/[controller]")]
public class FetcherController(ISeedingState seedingState) : ControllerBase
{
    /// <summary>
    /// Get the fetcher state.
    /// </summary>
    /// <returns>True if seeding, false if seeding is complete</returns>
    [HttpGet("seedingstate")]
    [AllowAnonymous]
    public IActionResult GetSeedingState()
    {
        return Ok(seedingState.IsSeeding);
    }
}