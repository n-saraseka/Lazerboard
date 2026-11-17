namespace Lazerboard.ExternalApis.Services.DirectApi;

public class DirectApiService(HttpClient httpClient, ILogger<DirectApiService> logger, DirectApiLimiter centralizedRateLimiter)
{
    private const string BaseApiV2Url = "https://osu.direct/api/v2";
    private const string BaseApiV1Url = "https://osu.direct/api";

    /// <summary>
    /// Sends a request to the API within the rate limit
    /// </summary>
    /// <param name="method">HTTP method (either HttpMethod.Get or HttpMethod.Post)</param>
    /// <param name="requestString">Request URL</param>
    /// <param name="content">Request content (for POST requests)</param>
    /// <param name="priority">Request priority</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Request response text</returns>
    private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, 
        string requestString,
        HttpContent? content,
        decimal priority = 1,
        CancellationToken ct = default)
    {
        var requestMessage = new HttpRequestMessage(method, requestString);
        requestMessage.Content = content;

        await centralizedRateLimiter.WaitForAvailableTokenAsync(priority, ct);
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
            $"{BaseApiV2Url}/search?q={dbQuery}&{sort}&{offsetString}", 
            null,
            2,
            ct);

        return await beatmapsetsResponse.Content.ReadAsStringAsync(ct);
    }
    
    /// <summary>
    /// Download a map from the API
    /// </summary>
    /// <param name="beatmapId">The beatmap ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The beatmap file <see cref="Stream"/></returns>
    public async Task<Stream> DownloadBeatmapAsync(int beatmapId, CancellationToken ct = default)
    {
        try
        {
            await centralizedRateLimiter.WaitForAvailableTokenAsync(1, ct);
            var requestString = $"{BaseApiV1Url}/osu/{beatmapId}";
            logger.Log(LogLevel.Information, "Request to osu.direct API: {requestString}", requestString);
            return await httpClient.GetStreamAsync(requestString, ct);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, ex, "Failed to download the beatmap");
            throw;
        }
    }
}