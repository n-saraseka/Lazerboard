using Newtonsoft.Json;
using System.Text;
using OsuScoreStats.OsuApi.OsuApiEntities;
using OsuScoreStats.OsuApi.Enums;

namespace OsuScoreStats.OsuApi;

public class OsuApiService(
    IHttpClientFactory httpClientFactory, 
    IConfiguration config, 
    ILogger<OsuApiService> logger)
{
    private static TokenInfo? _token;
    private static readonly SemaphoreSlim TokenSemaphore = new(1, 1);
    
    /// <summary>
    /// Sends a request to the API within the osu! API rate limit
    /// </summary>
    /// <param name="method">HTTP method (either HttpMethod.Get or HttpMethod.Post)</param>
    /// <param name="requestString">Request URL</param>
    /// <param name="content">Request content (for Post requests)</param>
    /// <param name="isTokenRequest">Whether the request is a token request or not</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Request response text</returns>
    private async Task<string> SendRequestAsync(HttpMethod method, 
        string requestString,
        HttpContent? content,
        bool isTokenRequest = false,
        CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient();
        var requestMessage = new HttpRequestMessage(method, requestString);
        requestMessage.Content = content;
        if (!isTokenRequest)
        {
            var tokenData = await GetValidTokenAsync(ct);
            requestMessage.Headers.Add("Authorization", "Bearer " + tokenData.AccessToken);
            requestMessage.Headers.Add("x-api-version", config["ApiVersion"]);
        }
        var responseText = "";
        
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var response = await client.SendAsync(requestMessage, ct);
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(1);
                    await Task.Delay(retryAfter, ct);
                }

                responseText = await response.Content.ReadAsStringAsync(ct);
                break;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Method: OsuApiService.SendRequestAsync; Method: {@method}; RequestString: {requestString}; Content: {@content}", 
                    method, requestString, content);
                throw;
            }
        }
        return responseText;
    }
    
    /// <summary>
    /// Set fresh token data for API access
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    private async Task SetTokenAsync(CancellationToken ct = default)
    {
        var seconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        var data = new Dictionary<string, string>();
        data.Add("client_id", config["ApiId"]);
        data.Add("client_secret", config["ApiSecret"]);
        data.Add("grant_type", "client_credentials");
        data.Add("scope", "public");
        var dataJson = JsonConvert.SerializeObject(data, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore});
        
        // getting the token
        var tokenResponse = await SendRequestAsync(HttpMethod.Post, 
            config["ApiTokenUrl"], 
            new StringContent(dataJson, Encoding.UTF8, "application/json"),
            true,
            ct);

        // writing new token data
        _token = JsonConvert.DeserializeObject<TokenInfo>(tokenResponse, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        _token.ExpiresIn += seconds;
    }

    /// <summary>
    /// Get beatmapsets from the API beatmapsets search endpoint (sorted by date ranked, ascending)
    /// </summary>
    /// <param name="cursor">Cursor string</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated BeatmapsetsResponse object</returns>
    public async Task<BeatmapsetsResponse> GetBeatmapsetsAsync(string? cursor, CancellationToken ct = default)
    {
        var beatmapsetsResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{config["BaseApiUrl"]}/beatmapsets/search?sort=ranked_asc&cursor_string={cursor}", 
            null, 
            false, 
            ct);
        
        var beatmapsets = JsonConvert.DeserializeObject<BeatmapsetsResponse>(beatmapsetsResponse, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        return beatmapsets;
    }

    /// <summary>
    /// Get beatmap scores from the API
    /// </summary>
    /// <param name="beatmapId">Beatmap ID</param>
    /// <param name="mode">Ruleset (osu, taiko, fruits, mania)</param>
    /// <param name="legacyOnly">Whether to exclude lazer scores or not (0 = include, 1 = exclude)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated BeatmapScores object</returns>
    public async Task<BeatmapScores> GetBeatmapScoresAsync(int beatmapId, Mode? mode, int legacyOnly = 0, CancellationToken ct = default)
    {
        legacyOnly = (legacyOnly < 0 || legacyOnly > 1) ? 0 : legacyOnly;
        var queryString = $"limit=100&legacy_only={legacyOnly}";
        if (mode != null) queryString += $"&mode={mode.ToString().ToLower()}";
        
        var scoresResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{config["BaseApiUrl"]}/beatmaps/{beatmapId}/scores?{queryString}", 
            null, 
            false, 
            ct);
        
        var scores = JsonConvert.DeserializeObject<BeatmapScores>(scoresResponse, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        return scores;
    }
    
    /// <summary>
    /// Get scores from the API firehose
    /// </summary>
    /// <param name="cursor">Cursor string (used to fetch new scores since last call)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated ScoresResponse object with the cursor string and array of Scores</returns>
    public async Task<ScoresResponse> GetScoresAsync(string? cursor, CancellationToken ct = default)
    {
        var scoresResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{config["BaseApiUrl"]}/scores?cursor_string={cursor}", 
            null, 
            false, 
            ct);

        var scores = JsonConvert.DeserializeObject<ScoresResponse>(scoresResponse, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        
        return scores;
    }
    
    /// <summary>
    /// Download a map from the API and save it to the cache folder
    /// </summary>
    /// <param name="beatmapId">The beatmap ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Parsed Beatmap object</returns>
    public async Task DownloadBeatmapAsync(int beatmapId, CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient();
        var mapPath = $"{config["CacheFolder"]}/{beatmapId}.osu";
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, 
                $"https://osu.ppy.sh/osu/{beatmapId}");
            var response = await client.SendAsync(requestMessage, ct);
            var responseBytes = await response.Content.ReadAsByteArrayAsync(ct);
                    
            await File.WriteAllBytesAsync(mapPath, responseBytes, ct);
                
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, ex, "Method: DownloadBeatmapAsync");
            throw;
        }
    }

    /// <summary>
    /// Get API Beatmap data from their IDs
    /// </summary>
    /// <param name="ids">List containing beatmap IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List with populated APIBeatmap objects</returns>
    public async Task<APIBeatmap[]> GetBeatmapsAsync(List<int> ids, CancellationToken ct = default)
    {

        int count = ids.Count;
        if (count == 0) throw new ArgumentException("No beatmap IDs to process");
        if (count > 50) throw new ArgumentException("ID limit per call reached (more than 50)");

        string queryString = string.Join("&", ids.Select(b => $"ids[]={b}"));

        // parse beatmaps
        string beatmapsResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{config["BaseApiUrl"]}/beatmaps?{queryString}", 
            null, 
            false, 
            ct);

        APIBeatmap[] beatmaps = JsonConvert.DeserializeObject<Dictionary<string, APIBeatmap[]>>(beatmapsResponse, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })["beatmaps"];

        logger.Log(LogLevel.Information, "Beatmaps received: {beatmapsCount}", ids.Count);

        return beatmaps;
    }

    /// <summary>
    /// Get API User data from their IDs
    /// </summary>
    /// <param name="ids">List containing user IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List with populated APIUser objects</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<APIUser[]> GetUsersAsync(List<int> ids, CancellationToken ct = default)
    {

        int count = ids.Count;
        if (count == 0) throw new ArgumentException("No user IDs to process");
        if (count > 50) throw new ArgumentException("ID limit per call reached (more than 50)");

        string queryString = string.Join("&", ids.Select(u => $"ids[]={u}"));
        
        // parse users
        string usersResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{config["BaseApiUrl"]}/users?{queryString}", 
            null, 
            false, 
            ct);

        APIUser[] users = JsonConvert.DeserializeObject<Dictionary<string, APIUser[]>>(usersResponse, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })["users"];

        logger.Log(LogLevel.Information, "Users received: {usersCount}", ids.Count);

        return users;
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