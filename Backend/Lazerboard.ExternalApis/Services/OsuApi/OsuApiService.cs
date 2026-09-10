using System.Text;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Newtonsoft.Json;

namespace Lazerboard.ExternalApis.Services.OsuApi;

public class OsuApiService
{
    private readonly HttpClient _httpClient;
    private ILogger<OsuApiService> _logger;
    private readonly OsuRateLimiter _centralizedRateLimiter;
    private static TokenInfo? _token;
    private static readonly SemaphoreSlim TokenSemaphore = new(1, 1);
    private const string BaseApiUrl = "https://osu.ppy.sh/api/v2";
    private const string ApiTokenUrl = "https://osu.ppy.sh/oauth/token";
    private const int ApiVersion = 20220705;
    private readonly string _apiClientId;
    private readonly string _apiClientSecret;
    
    public OsuApiService(HttpClient httpClient, 
        ILogger<OsuApiService> logger, 
        IConfiguration config, 
        OsuRateLimiter centralizedRateLimiter)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        var externalApisConfig = config.GetSection("ExternalApis");
        var osuApiConfig = externalApisConfig.GetSection("OsuApi");
        _apiClientId = osuApiConfig["ClientId"];
        _apiClientSecret = osuApiConfig["ClientSecret"];
        _centralizedRateLimiter = centralizedRateLimiter;
    }
    
    /// <summary>
    /// Sends a request to the API within the osu! API rate limit
    /// </summary>
    /// <param name="method">HTTP method (either HttpMethod.Get or HttpMethod.Post)</param>
    /// <param name="requestString">Request URL</param>
    /// <param name="content">Request content (for Post requests)</param>
    /// <param name="isTokenRequest">Whether the request is a token request or not</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Request response text</returns>
    private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, 
        string requestString,
        HttpContent? content,
        bool isTokenRequest = false,
        CancellationToken ct = default)
    {
        var requestMessage = new HttpRequestMessage(method, requestString);
        requestMessage.Content = content;
        if (!isTokenRequest)
        {
            var tokenData = await GetValidTokenAsync(ct);
            requestMessage.Headers.Add("Authorization", "Bearer " + tokenData.AccessToken);
            requestMessage.Headers.Add("x-api-version", ApiVersion.ToString());
        }

        await _centralizedRateLimiter.WaitForAvailableTokenAsync(ct);
        _logger.Log(LogLevel.Information, "Request to osu! API: {requestString}", requestString);
        var response = await _httpClient.SendAsync(requestMessage, ct);
        
        return response;
    }
    
    /// <summary>
    /// Set fresh token data for API access
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    private async Task SetTokenAsync(CancellationToken ct = default)
    {
        var seconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        var data = new Dictionary<string, string>();
        data.Add("client_id", _apiClientId);
        data.Add("client_secret", _apiClientSecret);
        data.Add("grant_type", "client_credentials");
        data.Add("scope", "public");
        var dataJson = JsonConvert.SerializeObject(data, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore});
        
        // getting the token
        using var tokenResponse = await SendRequestAsync(HttpMethod.Post, 
            ApiTokenUrl, 
            new StringContent(dataJson, Encoding.UTF8, "application/json"),
            true,
            ct);
        
        var tokenText = await tokenResponse.Content.ReadAsStringAsync(ct);

        // writing new token data
        _token = JsonConvert.DeserializeObject<TokenInfo>(tokenText, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        _token.ExpiresIn += seconds;
    }

    /// <summary>
    /// Get beatmapsets from the API beatmapsets search endpoint (sorted by date ranked, ascending)
    /// </summary>
    /// <param name="cursor">Cursor string</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The JSON string</returns>
    public async Task<string> GetBeatmapsetsAsync(string? cursor, CancellationToken ct = default)
    {
        using var beatmapsetsResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{BaseApiUrl}/beatmapsets/search?sort=ranked_asc&cursor_string={cursor}", 
            null, 
            false, 
            ct);

        return await beatmapsetsResponse.Content.ReadAsStringAsync(ct);
    }

    /// <summary>
    /// Get beatmapset data from the API
    /// </summary>
    /// <param name="id">The <see cref="APIBeatmapset"/> ID</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>The JSON string</returns>
    public async Task<string> GetBeatmapsetAsync(int id, CancellationToken ct = default)
    {
        using var beatmapsetResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{BaseApiUrl}/beatmapsets/{id}", 
            null, 
            false, 
            ct);
        
        return await beatmapsetResponse.Content.ReadAsStringAsync(ct);
    }

    /// <summary>
    /// Get beatmap scores from the API
    /// </summary>
    /// <param name="beatmapId">Beatmap ID</param>
    /// <param name="mode">Ruleset (osu, taiko, fruits, mania)</param>
    /// <param name="legacyOnly">Whether to exclude lazer scores or not (0 = include, 1 = exclude)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The JSON string</returns>
    public async Task<string> GetBeatmapScoresAsync(int beatmapId, Mode? mode, int legacyOnly = 0, CancellationToken ct = default)
    {
        legacyOnly = (legacyOnly < 0 || legacyOnly > 1) ? 0 : legacyOnly;
        var queryString = $"limit=100&legacy_only={legacyOnly}";
        if (mode != null) queryString += $"&mode={mode.ToString().ToLower()}";
        
        using var scoresResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{BaseApiUrl}/beatmaps/{beatmapId}/scores?{queryString}", 
            null, 
            false, 
            ct);
        
        return await scoresResponse.Content.ReadAsStringAsync(ct);
    }
    
    /// <summary>
    /// Get scores from the API firehose
    /// </summary>
    /// <param name="cursor">Cursor string (used to fetch new scores since last call)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The JSON string</returns>
    public async Task<string> GetScoresAsync(string? cursor, CancellationToken ct = default)
    {
        using var scoresResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{BaseApiUrl}/scores?cursor_string={cursor}", 
            null, 
            false, 
            ct);
        
        return await scoresResponse.Content.ReadAsStringAsync(ct);
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
            await _centralizedRateLimiter.WaitForAvailableTokenAsync(ct);
            var requestString = $"https://osu.ppy.sh/osu/{beatmapId}";
            _logger.Log(LogLevel.Information, "Request to osu! API: {requestString}", requestString);
            return await _httpClient.GetStreamAsync(requestString, ct);
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, ex, "Failed to download the beatmap");
            throw;
        }
    }

    /// <summary>
    /// Get API Beatmap data from their IDs
    /// </summary>
    /// <param name="ids">List containing beatmap IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The JSON string</returns>
    public async Task<string> GetBeatmapsAsync(IList<int> ids, CancellationToken ct = default)
    {
        var count = ids.Count;
        if (count == 0) throw new ArgumentException("No beatmap IDs to process");
        if (count > 50) throw new ArgumentException("ID limit per call reached (more than 50)");

        var queryString = string.Join("&", ids.Select(b => $"ids[]={b}"));

        // parse beatmaps
        using var beatmapsResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{BaseApiUrl}/beatmaps?{queryString}", 
            null, 
            false, 
            ct);
        
        return await beatmapsResponse.Content.ReadAsStringAsync(ct);
    }

    /// <summary>
    /// Get API User data from their IDs
    /// </summary>
    /// <param name="ids">List containing user IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The JSON string</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<string> GetUsersAsync(IList<int> ids, CancellationToken ct = default)
    {
        var count = ids.Count;
        if (count == 0) throw new ArgumentException("No user IDs to process");
        if (count > 50) throw new ArgumentException("ID limit per call reached (more than 50)");

        var queryString = string.Join("&", ids.Select(u => $"ids[]={u}"));
        
        using var usersResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{BaseApiUrl}/users?{queryString}", 
            null, 
            false, 
            ct);
        
        return await usersResponse.Content.ReadAsStringAsync(ct);
    }

    /// <summary>
    /// Check if token has expired
    /// </summary>
    /// <param name="ct">Cancellation Token</param>
    /// <returns></returns>
    private async Task<TokenInfo> GetValidTokenAsync(CancellationToken ct)
    {
        await TokenSemaphore.WaitAsync(ct);
        try
        {
            var seconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (_token == null || seconds > _token.ExpiresIn - 60)
                await SetTokenAsync(ct);
            return _token;
        }
        finally
        {
            TokenSemaphore.Release();
        }
    }
}