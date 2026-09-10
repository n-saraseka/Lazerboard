namespace Lazerboard.ExternalApis.Services.DirectApi;

public class DirectApiService(HttpClient httpClient, ILogger<DirectApiService> logger, DirectApiLimiter centralizedRateLimiter)
{
    private const string BaseApiUrl = "https://osu.direct/api/v2";

    /// <summary>
    /// Sends a request to the API within the osu! API rate limit
    /// </summary>
    /// <param name="method">HTTP method (either HttpMethod.Get or HttpMethod.Post)</param>
    /// <param name="requestString">Request URL</param>
    /// <param name="content">Request content (for Post requests)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Request response text</returns>
    private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, 
        string requestString,
        HttpContent? content,
        CancellationToken ct = default)
    {
        var requestMessage = new HttpRequestMessage(method, requestString);
        requestMessage.Content = content;

        await centralizedRateLimiter.WaitForAvailableTokenAsync(ct);
        logger.Log(LogLevel.Information, "Request to osu.direct API: {requestString}", requestString);
        var response = await httpClient.SendAsync(requestMessage, ct);
        
        return response;
    }
    
    /// <summary>
    /// Get unlisted beatmapsets
    /// </summary>
    /// <param name="offset">The offset of the results</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The JSON string</returns>
    public async Task<string> GetUnlistedBeatmapsetsAsync(int offset, CancellationToken ct = default)
    {
        var availabilityQuery =
            "(availability.more_information%20IS%20NOT%20NULL%20OR%20availability.download_disabled=true)";
        var rankStatusQuery = "(ranked=1%20OR%20ranked=2%20OR%20ranked=4)";
        var dbQuery = $"[{availabilityQuery}%20AND%20{rankStatusQuery}]";
        var sort = "sort=ranked_date:asc";
        var offsetString = $"offset={offset}";
        using var beatmapsetsResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{BaseApiUrl}/search?q={dbQuery}&{sort}&{offsetString}", 
            null,
            ct);

        return await beatmapsetsResponse.Content.ReadAsStringAsync(ct);
    }
}