using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.ScoreFetcher.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.BackgroundServices.ScanServices;

public class FirehoseLeaderboardsScanService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FirehoseLeaderboardsScanService> _logger;
    private ISeedingState _seedingState;

    private const int BatchSize = 50;
    private bool _shouldFinishAfterThisBatch;
    private int? _latestMapId;
    
    public FirehoseLeaderboardsScanService(IServiceProvider serviceProvider, ILogger<FirehoseLeaderboardsScanService> logger, ISeedingState seedingState)
    {
        _serviceProvider = serviceProvider;
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
                    using var scope = _serviceProvider.CreateScope();
                    var beatmapUtils = scope.ServiceProvider.GetRequiredService<IBeatmapUtils>();
                    
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
                        await beatmapUtils.ProcessLeaderboardAsync(beatmap.Id, mode, stoppingToken);
                    }
                }

                await SaveScanTimestampAsync(beatmapIds, stoppingToken);
                
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
        using var scope = _serviceProvider.CreateScope();
        var beatmapRepository = scope.ServiceProvider.GetRequiredService<IBeatmapRepository>();
        return await beatmapRepository.GetAll()
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
        using var scope = _serviceProvider.CreateScope();
        var beatmapRepository = scope.ServiceProvider.GetRequiredService<IBeatmapRepository>();
        
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetScanLogRepository>();
        var latestStartTimestamp = await scanLogsRepository.GetLatestStartedBeatmapScanAsync(stoppingToken);
        var latestFinishTimeStamp = await scanLogsRepository.GetLatestFinishedBeatmapScanAsync(stoppingToken);
            
        if (latestStartTimestamp is null 
            || (latestFinishTimeStamp != null && latestFinishTimeStamp.LoggedAt > latestStartTimestamp.LoggedAt))
        {
            await scanLogsRepository.SaveEventAsync(ScanEventType.BeatmapScanStarted, stoppingToken);
            _logger.Log(LogLevel.Information, "Started rescanning leaderboards with firehose scores at {datetime}", DateTime.UtcNow);
            return;
        }

        var latestRescannedMap = await beatmapRepository.GetLatestScannedBeatmapAsync(stoppingToken);
        
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
        using var scope = _serviceProvider.CreateScope();
        var beatmapRepository = scope.ServiceProvider.GetRequiredService<IBeatmapRepository>();
        var latestMapWithFirehoseScores = await beatmapRepository.GetLatestBeatmapWithFirehoseScoresAsync(stoppingToken);
        return latestMapWithFirehoseScores;
    }
    
    /// <summary>
    /// Save the <see cref="ScanEventType.ScanFinished"/> event
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task FinishScanningAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetScanLogRepository>();
        await scanLogsRepository.SaveEventAsync(ScanEventType.BeatmapScanFinished, stoppingToken);
        _seedingState.IsSeeding = false;
        _logger.Log(LogLevel.Information, "Rescan complete");
    }

    /// <summary>
    /// Save the scant timestamp for a list of <see cref="Beatmap.Id"/>s
    /// </summary>
    /// <param name="beatmapIds">The list of <see cref="Beatmap.Id"/>s</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task SaveScanTimestampAsync(IList<int> beatmapIds, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var utils = scope.ServiceProvider.GetRequiredService<IBeatmapUtils>();
        await utils.SaveBeatmapScansTimestampsAsync(beatmapIds, stoppingToken);
    }
}