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
        
        var dbBeatmapset = await beatmapsetRepository.GetByIdAsync(beatmapset.Id, stoppingToken);
        var currentDateTime = DateTimeOffset.Now;
        switch (eventType)
        {
            case ScanEventType.RescanStarted:
                dbBeatmapset!.FinishedScanningAt = currentDateTime;
                break;
            case ScanEventType.MainSeedingStarted:
                dbBeatmapset!.MainFinishedProcessingAt = currentDateTime;
                break;
            case ScanEventType.SecondarySeedingStarted:
                dbBeatmapset!.SecondaryFinishedProcessingAt = currentDateTime;
                break;
        }
        beatmapsetRepository.Update(dbBeatmapset);
        await beatmapsetRepository.SaveChangesAsync(stoppingToken);
    }
}