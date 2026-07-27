using OsuScoreStats.OsuApi;
using OsuScoreStats.OsuApi.Enums;
using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.ScoreFetcher;

public class ApiFetcher(OsuApiService osuApiService) : IApiFetcher
{
    /// <summary>
    /// Get beatmapsets from API and save beatmapset and beatmap data
    /// </summary>
    /// <param name="cursor">Cursor string</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated BeatmapsetsResponse object</returns>
    public async Task<BeatmapsetsResponse> SearchBeatmapsetsAsync(string? cursor, CancellationToken ct = default) =>
        await osuApiService.GetBeatmapsetsAsync(cursor, ct);

    public async Task<BeatmapScores> GetBeatmapScoresAsync(APIBeatmap beatmap, Mode? mode, int legacyOnly = 0, CancellationToken ct = default) =>
        await osuApiService.GetBeatmapScoresAsync(beatmap.Id, mode, legacyOnly, ct);
    
    /// <summary>
    /// Get scores from the API firehose
    /// </summary>
    /// <param name="cursor">Cursor string</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated ScoresResponse object</returns>
    public async Task<ScoresResponse> GetScoresAsync(string? cursor, CancellationToken ct = default)
    {
        return await osuApiService.GetScoresAsync(cursor, ct);
    }
    
    /// <summary>
    /// Get user data from API and process the respective data
    /// </summary>
    /// <param name="userIds">IEnumerable containing user IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    public async Task<List<APIUser>> GetUsersAsync(IEnumerable<int> userIds, CancellationToken ct = default)
    {
        const int batchSize = 50;
        var users = new List<APIUser>();
        
        if (userIds.Count() > 0)
        {
            for (int i = 0; i < userIds.Count(); i += batchSize)
            {
                var batch = userIds.Skip(i).Take(batchSize).ToList();
                APIUser[] userData = await osuApiService.GetUsersAsync(batch, ct);
                users.AddRange(userData);
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
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
    public async Task<List<APIBeatmap>> GetBeatmapsAsync(IEnumerable<int> beatmapIds, CancellationToken ct = default)
    {
        const int batchSize = 50;
        var beatmaps = new List<APIBeatmap>();
        
        for (int i = 0; i < beatmapIds.Count(); i += batchSize)
        {
            var batch = beatmapIds.Skip(i).Take(batchSize).ToList();
            APIBeatmap[] beatmapData = await osuApiService.GetBeatmapsAsync(batch, ct);
            beatmaps.AddRange(beatmapData);
            await Task.Delay(TimeSpan.FromSeconds(1), ct);
        }

        return beatmaps;
    }
}