using Lazerboard.ExternalApis.Services.NekohaApi;
using Microsoft.AspNetCore.Mvc;

namespace Lazerboard.ExternalApis.Apis;

[ApiController]
[Route("nekoha")]
public class NekohaApiController(NekohaApiService nekohaApiService) : ControllerBase
{
    /// <summary>
    /// Download a map from the API
    /// </summary>
    /// <param name="id">The beatmap ID</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("beatmaps/{id:int}/download")]
    public async Task<IActionResult> DownloadBeatmapAsync(int id, CancellationToken ct = default)
    {
        var stream = await nekohaApiService.DownloadBeatmapAsync(id, ct);
        return File(stream, "text/plain", $"{id}.osu");
    }
}