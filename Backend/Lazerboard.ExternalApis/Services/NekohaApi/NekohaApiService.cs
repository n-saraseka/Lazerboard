namespace Lazerboard.ExternalApis.Services.NekohaApi;

public class NekohaApiService(HttpClient httpClient, ILogger<NekohaApiService> logger, NekohaApiLimiter centralizedRateLimiter)
{
    private const string BaseApiUrl = "https://mirror.nekoha.moe/api";
    
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
            var requestString = $"{BaseApiUrl}/osu/{beatmapId}";
            logger.Log(LogLevel.Information, "Request to Nekoha API: {requestString}", requestString);
            return await httpClient.GetStreamAsync(requestString, ct);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, ex, "Failed to download the beatmap");
            throw;
        }
    }
}