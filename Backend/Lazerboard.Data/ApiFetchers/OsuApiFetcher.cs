using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Lazerboard.Data.ApiFetchers;

public class OsuApiFetcher : IOsuApiFetcher
{
    private readonly double _apiInterval;
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;
    
    public OsuApiFetcher(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        
        var externalApisConfig = config.GetSection("ExternalApis");
        var host = externalApisConfig.GetValue<string>("Host");
        var port = externalApisConfig.GetValue<int>("Port");
        
        var osuApiConfig = externalApisConfig.GetSection("OsuApi");
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
    /// Get beatmapsets from API and save beatmapset and beatmap data
    /// </summary>
    /// <param name="cursor">Cursor string</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated BeatmapsetsResponse object</returns>
    public async Task<BeatmapsetsResponse> SearchBeatmapsetsAsync(string? cursor, CancellationToken ct = default)
    {
        using var beatmapsetsResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{_apiUrl}/beatmapsets/search?sort=ranked_asc&cursor_string={cursor}", 
            null,
            ct);
        
        var beatmapsetsText = await beatmapsetsResponse.Content.ReadAsStringAsync(ct);
        var beatmapsets = JsonConvert.DeserializeObject<BeatmapsetsResponse>(beatmapsetsText, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

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
    public async Task<BeatmapScores> GetBeatmapScoresAsync(int beatmapId, Mode? mode, int legacyOnly = 0,
        CancellationToken ct = default)
    {
        legacyOnly = (legacyOnly < 0 || legacyOnly > 1) ? 0 : legacyOnly;
        var queryString = $"limit=100&legacy_only={legacyOnly}";
        if (mode != null) queryString += $"&mode={mode.ToString().ToLower()}";
        
        using var scoresResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{_apiUrl}/beatmaps/{beatmapId}/scores?{queryString}", 
            null,
            ct);
        
        var scoresResponseText = await scoresResponse.Content.ReadAsStringAsync(ct);
        
        var scores = JsonConvert.DeserializeObject<BeatmapScores>(scoresResponseText, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        return scores;
    }
    
    /// <summary>
    /// Get scores from the API firehose
    /// </summary>
    /// <param name="cursor">Cursor string</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated ScoresResponse object</returns>
    public async Task<ScoresResponse> GetScoresAsync(string? cursor, CancellationToken ct = default)
    {
        using var scoresResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{_apiUrl}/scores?cursor_string={cursor}", 
            null,
            ct);
        
        var scoresResponseText = await scoresResponse.Content.ReadAsStringAsync(ct);

        var scores = JsonConvert.DeserializeObject<ScoresResponse>(scoresResponseText, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        
        return scores;
    }
    
    /// <summary>
    /// Get user data from API and process the respective data
    /// </summary>
    /// <param name="userIds">IList containing user IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    public async Task<List<APIUser>> GetUsersAsync(IList<int> userIds, CancellationToken ct = default)
    {
        const int batchSize = 50;
        var users = new List<APIUser>();
        
        if (userIds.Count > 0)
        {
            for (int i = 0; i < userIds.Count; i += batchSize)
            {
                var batch = userIds.Skip(i).Take(batchSize).ToList();
                var queryString = string.Join("&", batch.Select(u => $"ids[]={u}"));
        
                using var usersResponse = await SendRequestAsync(HttpMethod.Get, 
                    $"{_apiUrl}/users?{queryString}", 
                    null,
                    ct);
        
                var usersResponseText = await usersResponse.Content.ReadAsStringAsync(ct);

                var userData = JsonConvert.DeserializeObject<Dictionary<string, APIUser[]>>(usersResponseText, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })["users"];
                
                users.AddRange(userData);
                await Task.Delay(TimeSpan.FromSeconds(_apiInterval), ct);
            }
        }

        return users;
    }
    
    /// <summary>
    /// Get beatmaps from API and process the data
    /// </summary>
    /// <param name="beatmapIds">IList containing beatmap IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    public async Task<List<APIBeatmap>> GetBeatmapsAsync(IList<int> beatmapIds, CancellationToken ct = default)
    {
        const int batchSize = 50;
        var beatmaps = new List<APIBeatmap>();
        
        for (int i = 0; i < beatmapIds.Count(); i += batchSize)
        {
            var batch = beatmapIds.Skip(i).Take(batchSize).ToList();
            var queryString = string.Join("&", batch.Select(b => $"ids[]={b}"));

            // parse beatmaps
            using var beatmapsResponse = await SendRequestAsync(HttpMethod.Get, 
                $"{_apiUrl}/beatmaps?{queryString}", 
                null, 
                ct);
        
            var beatmapsResponseText = await beatmapsResponse.Content.ReadAsStringAsync(ct);

            APIBeatmap[] beatmapData = JsonConvert.DeserializeObject<Dictionary<string, APIBeatmap[]>>(beatmapsResponseText, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })["beatmaps"];
            beatmaps.AddRange(beatmapData);
            await Task.Delay(TimeSpan.FromSeconds(_apiInterval), ct);
        }

        return beatmaps;
    }

    /// <summary>
    /// Get beatmapset data from the API
    /// </summary>
    /// <param name="beatmapsetId">The <see cref="APIBeatmapset"/> ID</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="APIBeatmapset"/></returns>
    public async Task<APIBeatmapset> GetBeatmapsetAsync(int beatmapsetId, CancellationToken ct = default)
    {
        using var beatmapsetResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{_apiUrl}/beatmapsets/{beatmapsetId}", 
            null,
            ct);
        
        var beatmapsetText = await beatmapsetResponse.Content.ReadAsStringAsync(ct);
        var beatmapset = JsonConvert.DeserializeObject<APIBeatmapset>(beatmapsetText, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        return beatmapset;
    }

    /// <summary>
    /// Download a map from the API
    /// </summary>
    /// <param name="beatmapId">The beatmap ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The beatmap file <see cref="Stream"/></returns>
    public async Task<Stream> DownloadBeatmapAsync(int beatmapId, CancellationToken ct = default)
    {
        using var beatmapFileResponse = await SendRequestAsync(HttpMethod.Get, 
            $"{_apiUrl}/beatmaps/{beatmapId}/download", 
            null,
            ct);
        
        return await beatmapFileResponse.Content.ReadAsStreamAsync(ct);
    }
}