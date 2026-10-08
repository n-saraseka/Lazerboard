using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Work;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.ScoreFetcher.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.BackgroundServices.ScanServices;

public class FirehoseLeaderboardsScanService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IBeatmapUtils _beatmapUtils;
    private readonly ILogger<FirehoseLeaderboardsScanService> _logger;
    private readonly ISeedingState _seedingState;

    private const int BatchSize = 50;
    private bool _shouldFinishAfterThisBatch;
    private int? _latestMapId;
    
    public FirehoseLeaderboardsScanService(IServiceProvider serviceProvider, 
        IBeatmapUtils beatmapUtils,
        IUnitOfWorkFactory unitOfWorkFactory,
        ILogger<FirehoseLeaderboardsScanService> logger, 
        ISeedingState seedingState)
    {
        _serviceProvider = serviceProvider;
        _unitOfWorkFactory = unitOfWorkFactory;
        _beatmapUtils = beatmapUtils;
        _logger = logger;
        _seedingState = seedingState;
        _seedingState.IsSeeding = true;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var finishingBeatmap = await GetFinishingBeatmapAsync(stoppingToken);
        _logger.Log(LogLevel.Information, "Finishing beatmap ID: {finishingMapId}", finishingBeatmap?.Id);
        await GetStartingDataAsync(stoppingToken);
        _logger.Log(LogLevel.Information, "Starting beatmap ID: {startingMapId}", _latestMapId);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var beatmaps = await GetBeatmapsAsync(stoppingToken);

                if (beatmaps.Count == 0)
                {
                    await FinishScanningAsync(stoppingToken);
                    break;
                }

                if (finishingBeatmap is not null && beatmaps.Select(b => b.Id).Contains(finishingBeatmap.Id))
                {
                    _shouldFinishAfterThisBatch = true;
                }

                _logger.Log(LogLevel.Information,
                    "Processing a batch of {beatmapCount} beatmaps between IDs {minId} and {maxId}",
                    beatmaps.Count, beatmaps.Min(b => b.Id), beatmaps.Max(b => b.Id));

                var beatmapIds = beatmaps.Select(b => b.Id).ToList();

                foreach (var beatmap in beatmaps)
                {
                    var groupedByModes = beatmap.Scores.GroupBy(s => s.Mode).ToList();
                    var relevantModes = new List<Mode>();
                    foreach (var group in groupedByModes)
                    {
                        if (group.Any(s => s.ScoreSource == ScoreSource.ScoreFetcher))
                        {
                            relevantModes.Add(group.Key);
                        }
                    }

                    foreach (var mode in relevantModes)
                    {
                        await _beatmapUtils.ProcessLeaderboardAsync(beatmap.Id, mode, stoppingToken);
                    }
                }

                await _beatmapUtils.SaveBeatmapScansTimestampsAsync(beatmapIds, stoppingToken);
                
                var latestMapset = beatmaps.MaxBy(b => b.Id);
                if (latestMapset is not null)
                {
                    _latestMapId = latestMapset.Id;
                }

                if (_shouldFinishAfterThisBatch)
                {
                    await FinishScanningAsync(stoppingToken);
                    break;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "Beatmapset seeding service failed!");
                throw;
            }
        }
    }
    
    /// <summary>
    /// Get a batch of <see cref="Beatmap"/>s from the database
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="Beatmap"/>s with their beatmaps</returns>
    private async Task<List<Beatmap>> GetBeatmapsAsync(CancellationToken stoppingToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        
        return await unitOfWork.Beatmaps.GetAll()
            .Where(b => b.Id > (_latestMapId ?? 0) && b.Scores.Any(s => s.ScoreSource == ScoreSource.ScoreFetcher))
            .OrderBy(b => b.Id)
            .Take(BatchSize)
            .Include(b => b.Scores)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(stoppingToken);
    }
    
    private async Task GetStartingDataAsync(CancellationToken stoppingToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        
        var latestStartTimestamp = await unitOfWork.BeatmapsetScanLogs.GetLatestStartedBeatmapScanAsync(stoppingToken);
        var latestFinishTimeStamp = await unitOfWork.BeatmapsetScanLogs.GetLatestFinishedBeatmapScanAsync(stoppingToken);
            
        if (latestStartTimestamp is null 
            || (latestFinishTimeStamp != null && latestFinishTimeStamp.LoggedAt > latestStartTimestamp.LoggedAt))
        {
            await unitOfWork.BeginTransactionAsync(stoppingToken);
            unitOfWork.BeatmapsetScanLogs.SaveEvent(ScanEventType.BeatmapScanStarted);
            await unitOfWork.CommitTransactionAsync(stoppingToken);
            
            _logger.Log(LogLevel.Information, "Started rescanning leaderboards with firehose scores at {datetime}", DateTime.UtcNow);
            return;
        }

        var latestRescannedMap = await unitOfWork.Beatmaps.GetLatestScannedBeatmapAsync(stoppingToken);
        
        if (latestRescannedMap is null) return;
        
        _latestMapId = latestRescannedMap.Id;
    }
    
    /// <summary>
    /// Get the latest processed mapset on which the seeding should finish
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns></returns>
    private async Task<Beatmap?> GetFinishingBeatmapAsync(CancellationToken stoppingToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        var latestMapWithFirehoseScores = await unitOfWork.Beatmaps.GetLatestBeatmapWithFirehoseScoresAsync(stoppingToken);
        return latestMapWithFirehoseScores;
    }
    
    /// <summary>
    /// Save the <see cref="ScanEventType.ScanFinished"/> event
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task FinishScanningAsync(CancellationToken stoppingToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        
        await unitOfWork.BeginTransactionAsync(stoppingToken);
        unitOfWork.BeatmapsetScanLogs.SaveEvent(ScanEventType.BeatmapScanFinished);
        await unitOfWork.CommitTransactionAsync(stoppingToken);
        
        _seedingState.IsSeeding = false;
        _logger.Log(LogLevel.Information, "Leaderboards scan complete");
    }
}