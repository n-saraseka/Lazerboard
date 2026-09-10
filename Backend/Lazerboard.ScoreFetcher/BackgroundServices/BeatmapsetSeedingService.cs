using System.Text;
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

public class BeatmapsetSeedingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BeatmapsetUpdatesService> _logger;
    private ISeedingState _seedingState;
    private bool _catchUpAfterRestart = true;
    
    private string? _cursor;
    
    public BeatmapsetSeedingService(IServiceProvider serviceProvider, ILogger<BeatmapsetUpdatesService> logger, ISeedingState seedingState)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        _seedingState = seedingState;
        _seedingState.IsSeeding = true;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var finishingBeatmapset = await GetFinishingBeatmapsetAsync(stoppingToken);
        _logger.Log(LogLevel.Information, "Finishing beatmapset ID: {beatmapsetId}", finishingBeatmapset?.Id);
        
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
                
                _logger.Log(LogLevel.Information, 
                    "Processing a batch of {beatmapsetCount} beatmapsets ranked between {minDate} and {maxDate}", 
                    beatmapsets.Count,
                    DateOnly.FromDateTime(beatmapsets.Min(bs => bs.RankedDate).Date),
                    DateOnly.FromDateTime(beatmapsets.Max(bs => bs.RankedDate).Date));

                using (var scope = _serviceProvider.CreateScope())
                {
                    var scoreFetchingUtils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();
                    await scoreFetchingUtils.SaveAllBeatmapsetDataAsync(beatmapsets, ScanEventType.RescanStarted, stoppingToken);
                }

                if (finishingBeatmapset is null)
                {
                    foreach (var beatmapset in beatmapsets)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var beatmapUtils = scope.ServiceProvider.GetRequiredService<IBeatmapUtils>();
                        await beatmapUtils.ProcessBeatmapsetAsync(beatmapset, ScanEventType.RescanStarted, stoppingToken);
                    }
                }

                if (finishingBeatmapset is not null && beatmapsets.Select(bs => bs.Id).Contains(finishingBeatmapset.Id))
                {
                    await FinishSeedingAsync(stoppingToken);
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "Leaderboard seeding service failed!");
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
        var startingBeatmapset = await GetStartingBeatmapsetAsync(stoppingToken);
        using var scope = _serviceProvider.CreateScope();
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IOsuApiFetcher>();
        
        if (startingBeatmapset is null)
        {
            _cursor = null;
        }
        else if (_catchUpAfterRestart)
        {
            if (startingBeatmapset.RankedDate is null)
            {
                var apiBeatmapset = await apiFetcher.GetBeatmapsetAsync(startingBeatmapset.Id, stoppingToken);
                startingBeatmapset.RankedDate = apiBeatmapset.RankedDate;
            }
            
            var approvedDate = startingBeatmapset.RankedDate.Value.ToUnixTimeMilliseconds();
            _cursor = Convert.ToBase64String(Encoding.Default.GetBytes($"{{\"approved_date\":{approvedDate},\"id\":{startingBeatmapset.Id}}}"));
            _catchUpAfterRestart = false;
        }
        
        var beatmapsetsResponse = await apiFetcher.SearchBeatmapsetsAsync(_cursor, stoppingToken);
        _cursor = beatmapsetsResponse.Cursor;
        
        return beatmapsetsResponse.Beatmapsets;
    }
    
    private async Task<Beatmapset?> GetStartingBeatmapsetAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var beatmapsetRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetRepository>();
        
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetScanLogRepository>();
        var latestStartTimestamp = await scanLogsRepository.GetLatestStartedScanAsync(stoppingToken);
        var latestFinishTimeStamp = await scanLogsRepository.GetLatestFinishedScanAsync(stoppingToken);
            
        if (latestStartTimestamp is null 
            || (latestFinishTimeStamp != null && latestFinishTimeStamp.LoggedAt > latestStartTimestamp.LoggedAt))
        {
            // Start seeding from the first beatmapset (DISCO PRINCE)
            await scanLogsRepository.SaveEventAsync(ScanEventType.RescanStarted, stoppingToken);
            return null;
        }
        
        return await beatmapsetRepository.GetLatestRescannedMapsetAsync(stoppingToken);
    }

    /// <summary>
    /// Get the beatmapset with null <see cref="Beatmapset.RankedDate"/> on which the seeding should finish
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns></returns>
    private async Task<Beatmapset?> GetFinishingBeatmapsetAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var beatmapsetRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetRepository>();
        return await beatmapsetRepository.GetLatestBeatmapsetWithNullRankAsync(stoppingToken);
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
        _logger.Log(LogLevel.Information, "Database seeding complete");
    }
}