using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.ScoreFetcher.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.BackgroundServices.ScanServices;

public class RescanService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RescanService> _logger;
    private ISeedingState _seedingState;

    private const int BatchSize = 50;
    private readonly Dictionary<Mode, bool> _topRemovalConfiguration = new();
    private bool _shouldFinishAfterThisBatch;
    private DateTimeOffset? _latestRankedDate;
    private int? _latestMapsetId;
    
    public RescanService(IServiceProvider serviceProvider, ILogger<RescanService> logger, ISeedingState seedingState)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _seedingState = seedingState;
        _seedingState.IsSeeding = true;
        
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var scanConfig = config.GetSection("RescanBehavior");
        
        var topsRemovalConfig = scanConfig.GetSection("RemoveScoresOutsideTop100");
        _topRemovalConfiguration[Mode.Osu] = topsRemovalConfig.GetValue<bool>("osu");
        _topRemovalConfiguration[Mode.Taiko] = topsRemovalConfig.GetValue<bool>("taiko");
        _topRemovalConfiguration[Mode.Fruits] = topsRemovalConfig.GetValue<bool>("fruits");
        _topRemovalConfiguration[Mode.Mania] = topsRemovalConfig.GetValue<bool>("mania");
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
                    await FinishSeedingAsync(stoppingToken);
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

                using (var scope = _serviceProvider.CreateScope())
                {
                    var beatmapUtils = scope.ServiceProvider.GetRequiredService<IBeatmapUtils>();
                    await beatmapUtils.SaveStartingTimestampAsync(beatmapsetIds, ScanEventType.RescanStarted, stoppingToken);
                }

                foreach (var beatmapset in beatmapsets)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var beatmapUtils = scope.ServiceProvider.GetRequiredService<IBeatmapUtils>();
                    await beatmapUtils.ProcessExistingMapsetAsync(beatmapset, 
                        ScanEventType.RescanStarted,
                        _topRemovalConfiguration, 
                        stoppingToken);
                }
                
                var latestMapset = beatmapsets.MaxBy(bs => bs.RankedDate);
                if (latestMapset is not null)
                {
                    _latestRankedDate = latestMapset.RankedDate;
                    _latestMapsetId = latestMapset.Id;
                }

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
    /// Get a batch of <see cref="Beatmapset"/>s from the database
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="Beatmapset"/>s with their beatmaps</returns>
    private async Task<List<Beatmapset>> GetBeatmapsetsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var beatmapsetRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetRepository>();
        return await beatmapsetRepository.GetAll()
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
        using var scope = _serviceProvider.CreateScope();
        var beatmapsetRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetRepository>();
        
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetScanLogRepository>();
        var latestStartTimestamp = await scanLogsRepository.GetLatestStartedScanAsync(stoppingToken);
        var latestFinishTimeStamp = await scanLogsRepository.GetLatestFinishedScanAsync(stoppingToken);
            
        if (latestStartTimestamp is null 
            || (latestFinishTimeStamp != null && latestFinishTimeStamp.LoggedAt > latestStartTimestamp.LoggedAt))
        {
            await scanLogsRepository.SaveEventAsync(ScanEventType.RescanStarted, stoppingToken);
            _logger.Log(LogLevel.Information, "Started scanning beatmapsets at {datetime}", DateTime.UtcNow);
            return;
        }
        
        var latestRescannedMapset = await beatmapsetRepository.GetLatestRescannedMapsetAsync(stoppingToken);
        
        if (latestRescannedMapset is null) return;
        
        if (latestRescannedMapset.RankedDate is null)
        {
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
        using var scope = _serviceProvider.CreateScope();
        var beatmapsetRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetRepository>();
        return await beatmapsetRepository.GetLatestMainProcessedMapsetAsync(stoppingToken);
    }
    
    /// <summary>
    /// Save the <see cref="ScanEventType.RescanFinished"/> event
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task FinishSeedingAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetScanLogRepository>();
        await scanLogsRepository.SaveEventAsync(ScanEventType.RescanFinished, stoppingToken);
        _seedingState.IsSeeding = false;
        _logger.Log(LogLevel.Information, "Rescan complete");
    }
}