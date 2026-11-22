using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lazerboard.Api;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/[controller]")]
public class FetcherController(HttpClient client): ControllerBase
{
    private const string BaseUrl = "http://scorefetcher:8080";
    
    /// <summary>
    /// Send request to the score fetcher service
    /// </summary>
    /// <param name="method">The <see cref="HttpMethod"/></param>
    /// <param name="requestString">The request string</param>
    /// <param name="content">The <see cref="HttpContent"/> or null</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="HttpResponseMessage"/></returns>
    private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, 
        string requestString,
        HttpContent? content,
        CancellationToken ct = default)
    {
        var requestMessage = new HttpRequestMessage(method, requestString);
        requestMessage.Content = content;
        
        var response = await client.SendAsync(requestMessage, ct);
        
        return response;
    }

    [HttpGet("seedingstate")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSeedingStateAsync(CancellationToken ct)
    {
        var requestString = $"{BaseUrl}/seedingstate";

        try
        {
            var response = await SendRequestAsync(HttpMethod.Get, requestString, null, ct);
            return Ok(await response.Content.ReadAsStringAsync(ct));
        }
        catch (Exception)
        {
            return StatusCode(503, "Score fetcher service unavailable");
        }
    }
}