using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Work;
using Lazerboard.ScoreFetcher.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.BackgroundServices.ScanServices;

public class RescanAllMapsService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IBeatmapUtils _beatmapUtils;
    private readonly ILogger<RescanAllMapsService> _logger;
    private ISeedingState _seedingState;

    private const int BatchSize = 50;
    private bool _shouldFinishAfterThisBatch;
    private DateTimeOffset? _latestRankedDate;
    private int? _latestMapsetId;
    
    public RescanAllMapsService(IServiceProvider serviceProvider,
        IUnitOfWorkFactory unitOfWorkFactory,
        IBeatmapUtils beatmapUtils,
        ILogger<RescanAllMapsService> logger, 
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
        var finishingBeatmapset = await GetFinishingBeatmapsetAsync(stoppingToken);
        _logger.Log(LogLevel.Information, "Finishing beatmapset ID: {finishingMapsetId}", finishingBeatmapset?.Id);
        await GetStartingDataAsync(stoppingToken);
        _logger.Log(LogLevel.Information, "Starting mapset ID: {startingMapsetId}", _latestMapsetId);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var beatmapsets = await GetBeatmapsetsAsync(stoppingToken);

                if (beatmapsets.Count == 0)
                {
                    await FinishScanningAsync(stoppingToken);
                    break;
                }

                if (finishingBeatmapset is not null &&
                    beatmapsets.Select(bs => bs.Id).Contains(finishingBeatmapset.Id))
                {
                    _shouldFinishAfterThisBatch = true;
                }
                
                _logger.Log(LogLevel.Information, 
                    "Processing a batch of {beatmapsetCount} beatmapsets ranked between {minDate} and {maxDate}", 
                    beatmapsets.Count,
                    DateOnly.FromDateTime(beatmapsets.Min(bs => bs.RankedDate!.Value).Date),
                    DateOnly.FromDateTime(beatmapsets.Max(bs => bs.RankedDate!.Value).Date));

                var beatmapsetIds = beatmapsets.Select(bs => bs.Id).ToList();

                await _beatmapUtils.SaveStartingTimestampAsync(beatmapsetIds, ScanEventType.ScanStarted, stoppingToken);

                foreach (var beatmapset in beatmapsets)
                {
                    await _beatmapUtils.ProcessExistingMapsetAsync(beatmapset, ScanEventType.ScanStarted, stoppingToken);
                }
                
                var latestMapset = beatmapsets.MaxBy(bs => bs.RankedDate);
                if (latestMapset is not null)
                {
                    _latestRankedDate = latestMapset.RankedDate;
                    _latestMapsetId = latestMapset.Id;
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
    /// Get a batch of <see cref="Beatmapset"/>s from the database
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="Beatmapset"/>s with their beatmaps</returns>
    private async Task<List<Beatmapset>> GetBeatmapsetsAsync(CancellationToken stoppingToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        
        return await unitOfWork.Beatmapsets.GetAll()
            .Where(bs => bs.RankedDate > (_latestRankedDate ?? DateTimeOffset.MinValue) 
                         || (bs.RankedDate == _latestRankedDate && bs.Id > _latestMapsetId))
            .OrderBy(bs => bs.RankedDate)
            .ThenBy(bs => bs.Id)
            .Take(BatchSize)
            .Include(bs => bs.Beatmaps)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(stoppingToken);
    }
    
    private async Task GetStartingDataAsync(CancellationToken stoppingToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        
        var latestStartTimestamp = await unitOfWork.BeatmapsetScanLogs.GetLatestStartedRescanAsync(stoppingToken);
        var latestFinishTimeStamp = await unitOfWork.BeatmapsetScanLogs.GetLatestFinishedRescanAsync(stoppingToken);
            
        if (latestStartTimestamp is null 
            || (latestFinishTimeStamp != null && latestFinishTimeStamp.LoggedAt > latestStartTimestamp.LoggedAt))
        {
            await unitOfWork.BeginTransactionAsync(stoppingToken);
            unitOfWork.BeatmapsetScanLogs.SaveEvent(ScanEventType.ScanStarted);
            await unitOfWork.CommitTransactionAsync(stoppingToken);
            
            _logger.Log(LogLevel.Information, "Started rescanning beatmapsets at {datetime}", DateTime.UtcNow);
            return;
        }
        
        var latestRescannedMapset = await unitOfWork.Beatmapsets.GetLatestRescannedMapsetAsync(stoppingToken);
        
        if (latestRescannedMapset is null) return;
        
        if (latestRescannedMapset.RankedDate is null)
        {
            using var scope = _serviceProvider.CreateScope();
            var apiFetcher = scope.ServiceProvider.GetRequiredService<IOsuApiFetcher>();
            var apiBeatmapset = await apiFetcher.GetBeatmapsetAsync(latestRescannedMapset.Id, stoppingToken);
            latestRescannedMapset.RankedDate = apiBeatmapset.RankedDate;
        }

        _latestRankedDate = latestRescannedMapset.RankedDate;
        _latestMapsetId = latestRescannedMapset.Id;
    }
    
    /// <summary>
    /// Get the latest processed mapset on which the seeding should finish
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns></returns>
    private async Task<Beatmapset?> GetFinishingBeatmapsetAsync(CancellationToken stoppingToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        
        var latestProcessedMapset = await unitOfWork.Beatmapsets.GetLatestMainProcessedMapsetAsync(stoppingToken);
        if (latestProcessedMapset != null) return latestProcessedMapset;
        return await unitOfWork.Beatmapsets.GetLatestScannedMapsetAsync(stoppingToken);
    }
    
    /// <summary>
    /// Save the <see cref="ScanEventType.ScanFinished"/> event
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task FinishScanningAsync(CancellationToken stoppingToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        
        await unitOfWork.BeginTransactionAsync(stoppingToken);
        unitOfWork.BeatmapsetScanLogs.SaveEvent(ScanEventType.ScanFinished);
        await unitOfWork.CommitTransactionAsync(stoppingToken);
        
        _seedingState.IsSeeding = false;
        _logger.Log(LogLevel.Information, "Rescan complete");
    }
}