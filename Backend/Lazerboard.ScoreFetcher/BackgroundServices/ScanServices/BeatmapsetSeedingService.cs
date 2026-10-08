using System.Text;
using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Work;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Lazerboard.ScoreFetcher.Processing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.BackgroundServices.ScanServices;

public class BeatmapsetSeedingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IScoreFetchingUtils _scoreFetchingUtils;
    private readonly IBeatmapUtils _beatmapUtils;
    private readonly ILogger<BeatmapsetSeedingService> _logger;
    private readonly ISeedingState _seedingState;

    private long _latestMapsetDateMs;
    private int _latestMapsetId;
    private readonly bool _onlyAddMissingMaps;
    private readonly bool _onlyUpdateMapsetData;
    private bool _shouldFinishAfterThisBatch;
    private string? _cursor;
    
    public BeatmapsetSeedingService(IServiceProvider serviceProvider,
        IScoreFetchingUtils scoreFetchingUtils,
        IConfiguration configuration,
        IBeatmapUtils beatmapUtils,
        IUnitOfWorkFactory unitOfWorkFactory,
        ILogger<BeatmapsetSeedingService> logger, 
        ISeedingState seedingState)
    {
        _serviceProvider = serviceProvider;
        _scoreFetchingUtils = scoreFetchingUtils;
        _unitOfWorkFactory = unitOfWorkFactory;
        _beatmapUtils = beatmapUtils;
        _logger = logger;
        _seedingState = seedingState;
        _seedingState.IsSeeding = true;

        var scanConfig = configuration.GetSection("ScanBehavior");
        _onlyAddMissingMaps = scanConfig.GetValue<bool>("OnlyAddMissingMaps");
        _onlyUpdateMapsetData = scanConfig.GetValue<bool>("OnlyUpdateMapsetData");
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var finishingBeatmapset = await GetFinishingBeatmapsetAsync(stoppingToken);
        _logger.Log(LogLevel.Information, "Finishing beatmapset ID: {beatmapsetId}", finishingBeatmapset?.Id);
        await GetStartingCursorAsync(stoppingToken);
        _logger.Log(LogLevel.Information, "Restart cursor for beatmapset seeding: {cursor}", _cursor);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var beatmapsets = await GetBeatmapsetsAsync(stoppingToken);

                if (beatmapsets.Count == 0)
                {
                    await FinishSeedingAsync(stoppingToken);
                    break;
                }

                if (finishingBeatmapset is not null &&
                    beatmapsets.Select(bs => bs.Id).Contains(finishingBeatmapset.Id))
                {
                    _shouldFinishAfterThisBatch = true;
                }

                if (_onlyAddMissingMaps)
                {
                    beatmapsets = await GetUnprocessedMapsetsAsync(beatmapsets, stoppingToken);
                    _logger.Log(LogLevel.Information, "Found {beatmapsetCount} new beatmapsets", beatmapsets.Count);
                    if (beatmapsets.Count == 0) continue;
                }
                
                _logger.Log(LogLevel.Information, 
                    "Processing a batch of {beatmapsetCount} beatmapsets ranked between {minDate} and {maxDate}", 
                    beatmapsets.Count,
                    DateOnly.FromDateTime(beatmapsets.Min(bs => bs.RankedDate).Date),
                    DateOnly.FromDateTime(beatmapsets.Max(bs => bs.RankedDate).Date));

                await _scoreFetchingUtils.SaveAllBeatmapsetDataAsync(beatmapsets, ScanEventType.RescanStarted, stoppingToken);

                if (_onlyUpdateMapsetData)
                {
                    var setIds = beatmapsets.Select(bs => bs.Id).ToList();
                    await _beatmapUtils.SaveFinishingTimestampAsync(setIds, ScanEventType.RescanStarted,
                        stoppingToken);
                }
                else
                {
                    foreach (var beatmapset in beatmapsets)
                    {
                        await _beatmapUtils.ProcessBeatmapsetAsync(beatmapset, ScanEventType.RescanStarted, stoppingToken);
                    }
                }
                
                var latestMapset = beatmapsets.MaxBy(bs => bs.RankedDate);
                _latestMapsetDateMs = latestMapset!.RankedDate.ToUnixTimeMilliseconds();
                _latestMapsetId = latestMapset.Id;

                if (_shouldFinishAfterThisBatch)
                {
                    await FinishSeedingAsync(stoppingToken);
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
    /// Get <see cref="APIBeatmapset"/>s from the search API
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="APIBeatmapset"/>s</returns>
    private async Task<List<APIBeatmapset>> GetBeatmapsetsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IOsuApiFetcher>();
        
        var beatmapsetsResponse = await apiFetcher.SearchBeatmapsetsAsync(_cursor, stoppingToken);
        _cursor = beatmapsetsResponse.Cursor;
        
        // Only happens when it's the last page of beatmapsets for some reason. We manually extract the correct cursor in that case.
        if (_cursor is null && _latestMapsetId > 1)
        {
            _cursor = Convert.ToBase64String(Encoding.Default.GetBytes($"{{\"approved_date\":{_latestMapsetDateMs},\"id\":{_latestMapsetId}}}"));
        }
        
        return beatmapsetsResponse.Beatmapsets;
    }
    
    private async Task GetStartingCursorAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = _unitOfWorkFactory.Create();
        
        var latestStartTimestamp = await unitOfWork.BeatmapsetScanLogs.GetLatestStartedScanAsync(stoppingToken);
        var latestFinishTimeStamp = await unitOfWork.BeatmapsetScanLogs.GetLatestFinishedScanAsync(stoppingToken);
            
        if (latestStartTimestamp is null 
            || (latestFinishTimeStamp != null && latestFinishTimeStamp.LoggedAt > latestStartTimestamp.LoggedAt))
        {
            // Start seeding from the first beatmapset (DISCO PRINCE)
            await unitOfWork.BeginTransactionAsync(stoppingToken);
            unitOfWork.BeatmapsetScanLogs.SaveEvent(ScanEventType.RescanStarted);
            await unitOfWork.CommitTransactionAsync(stoppingToken);
            
            _logger.Log(LogLevel.Information, "Started scanning beatmapsets at {datetime}", DateTime.UtcNow);
            return;
        }
        
        var latestRescannedMapset = await unitOfWork.Beatmapsets.GetLatestScannedMapsetAsync(stoppingToken);
        
        if (latestRescannedMapset is null) return;
        
        if (latestRescannedMapset.RankedDate is null)
        {
            var apiFetcher = scope.ServiceProvider.GetRequiredService<IOsuApiFetcher>();
            var apiBeatmapset = await apiFetcher.GetBeatmapsetAsync(latestRescannedMapset.Id, stoppingToken);
            latestRescannedMapset.RankedDate = apiBeatmapset.RankedDate;
        }
            
        var approvedDate = latestRescannedMapset.RankedDate.Value.ToUnixTimeMilliseconds();
        _latestMapsetDateMs = approvedDate;
        _latestMapsetId = latestRescannedMapset.Id;
        _cursor = Convert.ToBase64String(Encoding.Default.GetBytes($"{{\"approved_date\":{_latestMapsetDateMs},\"id\":{_latestMapsetId}}}"));
    }

    /// <summary>
    /// Get the beatmapset with null <see cref="Beatmapset.RankedDate"/> on which the seeding should finish
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns></returns>
    private async Task<Beatmapset?> GetFinishingBeatmapsetAsync(CancellationToken stoppingToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();

        if (_onlyUpdateMapsetData)
        {
            return await unitOfWork.Beatmapsets.GetLatestBeatmapsetWithNullRankAsync(stoppingToken);
        }

        if (_onlyAddMissingMaps)
        {
            return await unitOfWork.Beatmapsets.GetLatestMainProcessedMapsetAsync(stoppingToken);
        }

        return null;
    }

    /// <summary>
    /// Get mapsets that are either new or haven't finished processing yet from a batch
    /// </summary>
    /// <param name="beatmapsets">The <see cref="APIBeatmapset"/>s</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>The filtered <see cref="APIBeatmapset"/>s</returns>
    private async Task<List<APIBeatmapset>> GetUnprocessedMapsetsAsync(IList<APIBeatmapset> beatmapsets, CancellationToken stoppingToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        
        var beatmapsetIds = beatmapsets.Select(bs => bs.Id).ToList();
        
        var existingBeatmapsets = (await unitOfWork.Beatmapsets.GetBulkAsync(beatmapsetIds, stoppingToken))
            .Where(bs => bs.IsExplicit);
        var processedMapsets = existingBeatmapsets.Where(
                bs => bs.MainFinishedProcessingAt != null 
                      || bs.SecondaryFinishedProcessingAt != null
                      || bs.FinishedScanningAt != null)
            .ToList();
        var processedMapsetIds = processedMapsets.Select(bs => bs.Id).ToList();
        
        // At the moment we only have to care for missing explicit mapsets. That shouldn't change any time in the future, hopefully.
        return beatmapsets.Where(bs => !processedMapsetIds.Contains(bs.Id) && bs.IsExplicit).ToList();
    }

    /// <summary>
    /// Save the <see cref="ScanEventType.RescanFinished"/> event
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task FinishSeedingAsync(CancellationToken stoppingToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        
        await unitOfWork.BeginTransactionAsync(stoppingToken);
        unitOfWork.BeatmapsetScanLogs.SaveEvent(ScanEventType.RescanFinished);
        await unitOfWork.CommitTransactionAsync(stoppingToken);
        
        _seedingState.IsSeeding = false;
        _logger.Log(LogLevel.Information, "Database seeding complete");
    }
}