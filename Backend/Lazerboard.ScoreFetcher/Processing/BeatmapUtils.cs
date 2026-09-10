using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.Processing;

public class BeatmapUtils(ILogger<IBeatmapUtils> logger,
    IOsuApiFetcher osuApiFetcher, 
    IScoreFetchingUtils utils,
    IDataProcessor dataProcessor,
    IScoreProcessor scoreProcessor,
    IBeatmapsetRepository beatmapsetRepository) : IBeatmapUtils
{
    /// <summary>
    /// Get significant <see cref="APIBeatmap"/> leaderboard scores
    /// </summary>
    /// <param name="beatmapId">The <see cref="APIBeatmap"/> ID</param>
    /// <param name="mode">The <see cref="Mode"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task<List<APIScore>> GetBeatmapScoresAsync(int beatmapId, Mode mode, CancellationToken stoppingToken)
    {
        logger.Log(LogLevel.Information, "Processing beatmap {beatmapId}, mode: {mode}", beatmapId, mode);
        var beatmapScores = await osuApiFetcher.GetBeatmapScoresAsync(beatmapId, mode, 0, stoppingToken);
                        
        var significantScores = await utils.GetSignificantScoresAsync(beatmapScores.Scores, stoppingToken);
        return significantScores.DistinctBy(s => s.Id).ToList();
    }
    
    /// <summary>
    /// Process beatmapset maps and save the data
    /// </summary>
    /// <param name="beatmapset">The <see cref="APIBeatmapset"/></param>
    /// <param name="eventType">The <see cref="ScanEventType"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task ProcessBeatmapsetAsync(APIBeatmapset beatmapset, ScanEventType eventType, CancellationToken stoppingToken)
    {
        logger.Log(LogLevel.Information, "Processing beatmapset ID: {beatmapsetID}", beatmapset.Id);

        await dataProcessor.ProcessBeatmapsAsync(beatmapset.Beatmaps, stoppingToken);
        
        foreach (var beatmap in beatmapset.Beatmaps)
        {
            var flatWorkingBeatmap = await utils.GetFlatWorkingBeatmapAsync(beatmap.Id, stoppingToken);
            foreach (var val in Enum.GetValues<Mode>())
            {
                if (beatmap.Mode != Mode.Osu && val != beatmap.Mode) continue;
                var scores = await GetBeatmapScoresAsync(beatmap.Id, val, stoppingToken);

                if (scores.Count == 0) continue;
                
                var scoresWithoutPp = scores.Where(s => s.PP == null).ToList();
                var scoresWithPp = scores.Where(s => s.PP != null).ToList();
                        
                foreach (var score in scoresWithoutPp)
                {
                    await scoreProcessor.CalculateScoreAsync(score, flatWorkingBeatmap, stoppingToken);
                }
                
                var mergedScores = scoresWithPp.Concat(scoresWithoutPp).ToList();
                await utils.SaveScoreDataAsync(mergedScores, ScoreSource.LeaderboardScan, stoppingToken);
            }
        }
        
        List<APIBeatmapset> list = [beatmapset];
        await SaveProcessingTimestampAsync(list, eventType, stoppingToken);
    }

    /// <summary>
    /// Save the event timestamp for a list of <see cref="APIBeatmapset"/>s
    /// </summary>
    /// <param name="beatmapsets">The <see cref="APIBeatmapset"/>s</param>
    /// <param name="eventType">The <see cref="ScanEventType"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task SaveProcessingTimestampAsync(IList<APIBeatmapset> beatmapsets, ScanEventType eventType, CancellationToken stoppingToken)
    {
        var ids = beatmapsets.Select(bs => bs.Id).ToList();
        var dbBeatmapsets = await beatmapsetRepository.GetBulkAsync(ids, stoppingToken);
        var currentDateTime = DateTimeOffset.Now;
        foreach (var beatmapset in dbBeatmapsets)
        {
            switch (eventType)
            {
                case ScanEventType.RescanStarted:
                    beatmapset.FinishedScanningAt = currentDateTime;
                    break;
                case ScanEventType.MainSeedingStarted:
                    beatmapset.MainFinishedProcessingAt = currentDateTime;
                    break;
                case ScanEventType.SecondarySeedingStarted:
                    beatmapset.SecondaryFinishedProcessingAt = currentDateTime;
                    break;
            }
        }
        beatmapsetRepository.UpdateBulk(dbBeatmapsets);
        await beatmapsetRepository.SaveChangesAsync(stoppingToken);
    }
}