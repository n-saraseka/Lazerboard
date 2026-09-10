using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Lazerboard.ExternalApis.Services.DirectApi;
using Microsoft.AspNetCore.Mvc;

namespace Lazerboard.ExternalApis.Apis;

[ApiController]
[Route("osudotdirect")]
public class DirectApiController(DirectApiService directApiService) : ControllerBase
{
    /// <summary>
    /// Get beatmapsets from the unlisted beatmapsets endpoint
    /// </summary>
    /// <param name="offset">The offset of the results</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated <see cref="BeatmapsetsResponse"/> object</returns>
    [HttpGet("beatmapsets")]
    public async Task<IActionResult> GetBeatmapsetsAsync([FromQuery] int offset, CancellationToken ct = default)
    {
        var beatmapsetsResponse = await directApiService.GetUnlistedBeatmapsetsAsync(offset, ct);
        return Ok(beatmapsetsResponse);
    }
}