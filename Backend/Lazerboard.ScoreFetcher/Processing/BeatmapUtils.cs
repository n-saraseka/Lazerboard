using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Lazerboard.Data.Redis.Repositories.Interfaces;
using Lazerboard.ScoreFetcher.Calculations;
using osu.Game.Beatmaps;

namespace Lazerboard.ScoreFetcher.Processing;

public class BeatmapUtils(IOsuApiFetcher osuApiFetcher, 
    IScoreFetchingUtils utils,
    ICacheStore cacheStore,
    IBeatmapCacheRepository beatmapCacheRepository) : IBeatmapUtils
{
    /// <summary>
    /// Get significant <see cref="APIBeatmap"/> leaderboard scores
    /// </summary>
    /// <param name="beatmapId">The <see cref="APIBeatmap"/> ID</param>
    /// <param name="mode">The <see cref="Mode"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task<List<APIScore>> GetBeatmapScoresAsync(int beatmapId, Mode mode, CancellationToken stoppingToken)
    {
        var beatmapScores = await osuApiFetcher.GetBeatmapScoresAsync(beatmapId, mode, 0, stoppingToken);
                        
        var significantScores = await utils.GetSignificantScoresAsync(beatmapScores.Scores, stoppingToken);
        return significantScores.DistinctBy(s => s.Id).ToList();
    }

    /// <summary>
    /// Get the <see cref="FlatWorkingBeatmap"/> for <see cref="APIBeatmap"/> ID
    /// </summary>
    /// <param name="beatmapId">The <see cref="APIBeatmap"/> ID</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="FlatWorkingBeatmap"/></returns>
    public async Task<FlatWorkingBeatmap> GetFlatWorkingBeatmapAsync(int beatmapId, CancellationToken stoppingToken)
    {
        var filename = await cacheStore.GetBeatmapFileStringAsync(beatmapId, osuApiFetcher, beatmapCacheRepository, stoppingToken);
        return new FlatWorkingBeatmap(filename);
    }
}