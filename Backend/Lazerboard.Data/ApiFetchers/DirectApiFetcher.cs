using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lazerboard.Data.ApiFetchers;

public class DirectApiFetcher : IDirectApiFetcher
{
    private readonly double _apiInterval;
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;
    private readonly ILogger<IDirectApiFetcher> _logger;
    
    public DirectApiFetcher(HttpClient httpClient, IConfiguration config, ILogger<IDirectApiFetcher> logger)
    {
        _httpClient = httpClient;
        
        var externalApisConfig = config.GetSection("ExternalApis");
        var host = externalApisConfig.GetValue<string>("Host");
        var port = externalApisConfig.GetValue<int>("Port");
        
        var osuApiConfig = externalApisConfig.GetSection("OsuDotDirect");
        _apiInterval = osuApiConfig.GetValue<double>("ApiInterval");
        var baseUrl = osuApiConfig.GetValue<string>("BaseAddress");

        var builder = new UriBuilder
        {
            Scheme = "http",
            Host = host,
            Port = port,
            Path = baseUrl
        };
        
        _apiUrl = builder.Uri.ToString();

        _logger = logger;
    }
    
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
        
        var response = await _httpClient.SendAsync(requestMessage, ct);
        
        return response;
    }
    
    /// <summary>
    /// Get unlisted beatmapsets
    /// </summary>
    /// <param name="offset">The offset of the results</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>An array of <see cref="APIBeatmapset"/>s</returns>
    public async Task<APIBeatmapset[]> GetBeatmapsetsAsync(int offset, CancellationToken ct = default)
    {
        using var beatmapsetsResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{_apiUrl}/beatmapsets?offset={offset}",
            null,
            ct);
        
        var beatmapsetsText = await beatmapsetsResponse.Content.ReadAsStringAsync(ct);
        var beatmapsets = JsonConvert.DeserializeObject<APIBeatmapset[]>(beatmapsetsText, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        return beatmapsets;
    }
}