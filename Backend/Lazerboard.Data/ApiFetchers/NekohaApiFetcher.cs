using Microsoft.Extensions.Configuration;

namespace Lazerboard.Data.ApiFetchers;

public class NekohaApiFetcher : INekohaApiFetcher
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;
    
    public NekohaApiFetcher(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        
        var externalApisConfig = config.GetSection("ExternalApis");
        var host = externalApisConfig.GetValue<string>("Host");
        var port = externalApisConfig.GetValue<int>("Port");
        
        var osuApiConfig = externalApisConfig.GetSection("Nekoha");
        var baseUrl = osuApiConfig.GetValue<string>("BaseAddress");

        var builder = new UriBuilder
        {
            Scheme = "http",
            Host = host,
            Port = port,
            Path = baseUrl
        };
        
        _apiUrl = builder.Uri.ToString();
    }
    
    /// <summary>
    /// Download a map from the API
    /// </summary>
    /// <param name="beatmapId">The beatmap ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The beatmap file <see cref="Stream"/></returns>
    public async Task<Stream> DownloadBeatmapAsync(int beatmapId, CancellationToken ct = default) =>
        await _httpClient.GetStreamAsync($"{_apiUrl}/beatmaps/{beatmapId}/download", ct);
}