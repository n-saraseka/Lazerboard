using Microsoft.Extensions.Configuration;
using OsuScoreStats.Shared.OsuApi;
using OsuScoreStats.Shared.OsuApi.Enums;
using OsuScoreStats.Shared.OsuApi.OsuApiEntities;

namespace OsuScoreStats.Shared.Processing;

public class ApiFetcher : IApiFetcher
{
    private readonly double _apiInterval;
    private readonly OsuApiService _osuApiService;
    
    public ApiFetcher(OsuApiService osuApiService, IConfiguration config)
    {
        _osuApiService = osuApiService;
        _apiInterval = double.Parse(config["OsuApiInterval"]);
    }
    
    /// <summary>
    /// Get beatmapsets from API and save beatmapset and beatmap data
    /// </summary>
    /// <param name="cursor">Cursor string</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated BeatmapsetsResponse object</returns>
    public Task<BeatmapsetsResponse> SearchBeatmapsetsAsync(string? cursor, CancellationToken ct = default) =>
        _osuApiService.GetBeatmapsetsAsync(cursor, ct);

    public Task<BeatmapScores> GetBeatmapScoresAsync(APIBeatmap beatmap, Mode? mode, int legacyOnly = 0, CancellationToken ct = default) =>
        _osuApiService.GetBeatmapScoresAsync(beatmap.Id, mode, legacyOnly, ct);
    
    /// <summary>
    /// Get scores from the API firehose
    /// </summary>
    /// <param name="cursor">Cursor string</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated ScoresResponse object</returns>
    public Task<ScoresResponse> GetScoresAsync(string? cursor, CancellationToken ct = default) => _osuApiService.GetScoresAsync(cursor, ct);
    
    /// <summary>
    /// Get user data from API and process the respective data
    /// </summary>
    /// <param name="userIds">IEnumerable containing user IDs</param>
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
                APIUser[] userData = await _osuApiService.GetUsersAsync(batch, ct);
                users.AddRange(userData);
                await Task.Delay(TimeSpan.FromSeconds(_apiInterval), ct);
            }
        }

        return users;
    }
    
    /// <summary>
    /// Get beatmaps from API and process the data
    /// </summary>
    /// <param name="beatmapIds">IEnumerable containing beatmap IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    public async Task<List<APIBeatmap>> GetBeatmapsAsync(IList<int> beatmapIds, CancellationToken ct = default)
    {
        const int batchSize = 50;
        var beatmaps = new List<APIBeatmap>();
        
        for (int i = 0; i < beatmapIds.Count; i += batchSize)
        {
            var batch = beatmapIds.Skip(i).Take(batchSize).ToList();
            APIBeatmap[] beatmapData = await _osuApiService.GetBeatmapsAsync(batch, ct);
            beatmaps.AddRange(beatmapData);
            await Task.Delay(TimeSpan.FromSeconds(_apiInterval), ct);
        }

        return beatmaps;
    }
}