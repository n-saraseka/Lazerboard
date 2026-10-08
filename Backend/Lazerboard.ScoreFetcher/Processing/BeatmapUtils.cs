using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Work;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.Processing;

public class BeatmapUtils(IServiceProvider serviceProvider,
    IUnitOfWorkFactory unitOfWorkFactory,
    IScoreFetchingUtils utils,
    IDataProcessor dataProcessor,
    IScoreProcessor scoreProcessor,
    ILogger<IBeatmapUtils> logger) : IBeatmapUtils
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
        BeatmapScores beatmapScores;
        using (var scope = serviceProvider.CreateScope())
        {
            var osuApiFetcher = scope.ServiceProvider.GetRequiredService<IOsuApiFetcher>();
            beatmapScores = await osuApiFetcher.GetBeatmapScoresAsync(beatmapId, mode, 0, stoppingToken);
        }
                        
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
            foreach (var val in Enum.GetValues<Mode>())
            {
                if (beatmap.Mode != Mode.Osu && val != beatmap.Mode) continue;
                var scores = await GetBeatmapScoresAsync(beatmap.Id, val, stoppingToken);

                if (scores.Count == 0) continue;
                
                var scoresWithoutPp = scores.Where(s => s.PP == null).ToList();
                var scoresWithPp = scores.Where(s => s.PP != null).ToList();

                if (scoresWithoutPp.Count > 0)
                {
                    var flatWorkingBeatmap = await utils.GetFlatWorkingBeatmapAsync(beatmap.Id, stoppingToken);
                    foreach (var score in scoresWithoutPp)
                    {
                        await scoreProcessor.CalculateScoreAsync(score, flatWorkingBeatmap, stoppingToken);
                    }
                }
                
                var mergedScores = scoresWithPp.Concat(scoresWithoutPp).ToList();
                await utils.SaveScoreDataAsync(mergedScores, ScoreSource.LeaderboardScan, stoppingToken);
            }
        }
        
        await SaveFinishingTimestampAsync([beatmapset.Id], eventType, stoppingToken);
    }
    
    /// <summary>
    /// Process existing mapset and save the data
    /// </summary>
    /// <param name="beatmapset">The <see cref="APIBeatmapset"/></param>
    /// <param name="eventType">The <see cref="ScanEventType"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task ProcessExistingMapsetAsync(Beatmapset beatmapset, ScanEventType eventType, CancellationToken stoppingToken)
    {
        logger.Log(LogLevel.Information, "Processing beatmapset ID: {beatmapsetID}", beatmapset.Id);
        
        foreach (var beatmap in beatmapset.Beatmaps)
        {
            foreach (var val in Enum.GetValues<Mode>())
            {
                if (beatmap.Mode != Mode.Osu && val != beatmap.Mode) continue;
                await ProcessLeaderboardAsync(beatmap.Id, val, stoppingToken);
            }
        }
        
        await SaveFinishingTimestampAsync([beatmapset.Id], eventType, stoppingToken);
    }

    /// <summary>
    /// Process leaderboard scores and save the data
    /// </summary>
    /// <param name="beatmapId">The <see cref="APIBeatmap"/> ID</param>
    /// <param name="mode">The <see cref="Mode"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task ProcessLeaderboardAsync(int beatmapId, Mode mode, CancellationToken stoppingToken)
    {
        var scores = await GetBeatmapScoresAsync(beatmapId, mode, stoppingToken);

        if (scores.Count == 0) return;
                
        var scoresWithoutPp = scores.Where(s => s.PP == null).ToList();
        var scoresWithPp = scores.Where(s => s.PP != null).ToList();

        if (scoresWithoutPp.Count > 0)
        {
            var flatWorkingBeatmap = await utils.GetFlatWorkingBeatmapAsync(beatmapId, stoppingToken);
            foreach (var score in scoresWithoutPp)
            {
                await scoreProcessor.CalculateScoreAsync(score, flatWorkingBeatmap, stoppingToken);
            }
        }
                
        var mergedScores = scoresWithPp.Concat(scoresWithoutPp).ToList();
        await utils.SaveScoreDataAsync(mergedScores, ScoreSource.LeaderboardScan, stoppingToken);
    }
    
    /// <summary>
    /// Save the starting event timestamp for a list of <see cref="Beatmapset"/> IDs
    /// </summary>
    /// <param name="beatmapsetIds">The <see cref="Beatmapset"/> IDs</param>
    /// <param name="eventType">The <see cref="ScanEventType"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task SaveStartingTimestampAsync(IList<int> beatmapsetIds, ScanEventType eventType, CancellationToken stoppingToken)
    {
        var unitOfWork = unitOfWorkFactory.Create();
        var dbBeatmapsets = await unitOfWork.Beatmapsets.GetBulkAsync(beatmapsetIds, stoppingToken);
        var currentDateTime = DateTimeOffset.Now;
        foreach (var beatmapset in dbBeatmapsets)
        {
            switch (eventType)
            {
                case ScanEventType.RescanStarted:
                    beatmapset.StartedScanningAt = currentDateTime;
                    break;
                case ScanEventType.MainSeedingStarted:
                    beatmapset.MainStartedProcessingAt = currentDateTime;
                    break;
                case ScanEventType.SecondarySeedingStarted:
                    beatmapset.SecondaryStartedProcessingAt = currentDateTime;
                    break;
                case ScanEventType.ScanStarted:
                    beatmapset.StartedRescanningAt = currentDateTime;
                    break;
            }
        }

        await unitOfWork.BeginTransactionAsync(stoppingToken);
        unitOfWork.Beatmapsets.UpdateBulk(dbBeatmapsets);
        await unitOfWork.CommitTransactionAsync(stoppingToken);
    }

    /// <summary>
    /// Save the finishing event timestamp for a list of <see cref="Beatmapset"/> IDs
    /// </summary>
    /// <param name="beatmapsetIds">The <see cref="Beatmapset"/> IDs</param>
    /// <param name="eventType">The <see cref="ScanEventType"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task SaveFinishingTimestampAsync(IList<int> beatmapsetIds, ScanEventType eventType, CancellationToken stoppingToken)
    {
        var unitOfWork = unitOfWorkFactory.Create();
        var dbBeatmapsets = await unitOfWork.Beatmapsets.GetBulkAsync(beatmapsetIds, stoppingToken);
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
                case ScanEventType.ScanStarted:
                    beatmapset.FinishedRescanningAt = currentDateTime;
                    break;
            }
        }
        
        await unitOfWork.BeginTransactionAsync(stoppingToken);
        unitOfWork.Beatmapsets.UpdateBulk(dbBeatmapsets);
        await unitOfWork.CommitTransactionAsync(stoppingToken);
    }

    /// <summary>
    /// Save the scan event timestamp for a list of <see cref="Beatmap"/> IDs
    /// </summary>
    /// <param name="beatmapIds">The <see cref="Beatmap"/> IDs</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    public async Task SaveBeatmapScansTimestampsAsync(IList<int> beatmapIds, CancellationToken stoppingToken)
    {
        var unitOfWork = unitOfWorkFactory.Create();
        var dbBeatmaps = await unitOfWork.Beatmaps.GetBulkAsync(beatmapIds, stoppingToken);
        var currentDateTime = DateTime.UtcNow;
        
        dbBeatmaps = dbBeatmaps.Select(b =>
        {
            b.ScannedAt = currentDateTime;
            return b;
        }).ToList();
        
        await unitOfWork.BeginTransactionAsync(stoppingToken);
        unitOfWork.Beatmaps.UpdateBulk(dbBeatmaps);
        await unitOfWork.CommitTransactionAsync(stoppingToken);
    }
}