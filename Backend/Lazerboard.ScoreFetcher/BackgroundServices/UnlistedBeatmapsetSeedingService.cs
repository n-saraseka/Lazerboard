using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Lazerboard.ScoreFetcher.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.BackgroundServices;

public class UnlistedBeatmapsetSeedingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BeatmapsetUpdatesService> _logger;
    private ISeedingState _seedingState;
    private int _offset;
    
    public UnlistedBeatmapsetSeedingService(IServiceProvider serviceProvider, ILogger<BeatmapsetUpdatesService> logger, ISeedingState seedingState)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        _seedingState = seedingState;
        _seedingState.IsSeeding = true;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var startingBeatmapset = await GetStartingBeatmapsetAsync(stoppingToken);
        if (startingBeatmapset is not null)
        {
            _logger.Log(LogLevel.Information, "Starting beatmapset ID: {beatmapsetId}", startingBeatmapset.Id);
        }
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var beatmapsets = startingBeatmapset == null
                    ? await GetRelevantBeatmapsetBatchAsync(startingBeatmapset, stoppingToken) 
                    : await GetBeatmapsetsAsync(_offset, stoppingToken);
                
                var beatmapsetCount = beatmapsets.Count;

                if (startingBeatmapset is not null)
                {
                    var ids = beatmapsets.Select(bs => bs.Id).ToList();
                    beatmapsets = beatmapsets.Skip(ids.IndexOf(startingBeatmapset.Id) + 1).ToList();
                }
                
                if (beatmapsetCount == 0)
                {
                    await FinishSeedingAsync(stoppingToken);
                }
                else
                {
                    _logger.Log(LogLevel.Information, 
                        "Processing a batch of {beatmapsetCount} beatmapsets ranked between {minDate} and {maxDate}", 
                        beatmapsets.Count,
                        DateOnly.FromDateTime(beatmapsets.Min(bs => bs.RankedDate).Date),
                        DateOnly.FromDateTime(beatmapsets.Max(bs => bs.RankedDate).Date));

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var scoreFetchingUtils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();
                        await scoreFetchingUtils.SaveAllBeatmapsetDataAsync(beatmapsets, ScanEventType.MainSeedingStarted, stoppingToken);
                    }
                    
                    foreach (var beatmapset in beatmapsets)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var beatmapUtils = scope.ServiceProvider.GetRequiredService<IBeatmapUtils>();
                        await beatmapUtils.ProcessBeatmapsetAsync(beatmapset, ScanEventType.MainSeedingStarted, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "Unlisted leaderboard seeding service failed!");
                throw;
            }
        }
    }

    private async Task<List<APIBeatmapset>> GetRelevantBeatmapsetBatchAsync(Beatmapset? beatmapset,
        CancellationToken stoppingToken)
    {
        if (beatmapset is null) return await GetBeatmapsetsAsync(0, stoppingToken);
        var beatmapsets = await GetBeatmapsetsAsync(_offset, stoppingToken);
        var ids = beatmapsets.Select(b => b.Id).ToList();
        if (ids.Contains(beatmapset.Id)) return beatmapsets;
        _offset += beatmapsets.Count;
        return await GetBeatmapsetsAsync(_offset, stoppingToken);
    }

    private async Task<List<APIBeatmapset>> GetBeatmapsetsAsync(int offset, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IDirectApiFetcher>();
        return await apiFetcher.GetBeatmapsetsAsync(offset, stoppingToken);
    }
    
    private async Task<Beatmapset?> GetStartingBeatmapsetAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var beatmapsetRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetRepository>();
        
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetScanLogRepository>();
        var latestStartTimestamp = await scanLogsRepository.GetLatestStartedSecondarySeedingAsync(stoppingToken);
        var latestFinishTimeStamp = await scanLogsRepository.GetLatestFinishedSecondarySeedingAsync(stoppingToken);
            
        if (latestStartTimestamp is null 
            || (latestFinishTimeStamp != null && latestFinishTimeStamp.LoggedAt > latestStartTimestamp.LoggedAt))
        {
            // Start seeding from the first unlisted beatmapset
            await scanLogsRepository.SaveEventAsync(ScanEventType.SecondarySeedingStarted, stoppingToken);
            return null;
        }
        
        return await beatmapsetRepository.GetLatestSecondaryProcessedMapsetAsync(stoppingToken);
    }
    
    /// <summary>
    /// Save the <see cref="ScanEventType.RescanFinished"/> event
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task FinishSeedingAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetScanLogRepository>();
        await scanLogsRepository.SaveEventAsync(ScanEventType.SecondarySeedingFinished, stoppingToken);
        _seedingState.IsSeeding = false;
        _logger.Log(LogLevel.Information, "Database seeding complete");
    }
}