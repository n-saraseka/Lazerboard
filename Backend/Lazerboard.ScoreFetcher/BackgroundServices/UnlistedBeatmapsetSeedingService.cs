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

public class UnlistedBeatmapsetSeedingService(
    IServiceProvider serviceProvider,
    ILogger<UnlistedBeatmapsetSeedingService> logger,
    ISeedingState seedingState)
    : BackgroundService
{
    private int _offset;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var startingBeatmapset = await GetStartingBeatmapsetAsync(stoppingToken);
        if (startingBeatmapset is not null)
        {
            logger.Log(LogLevel.Information, "Starting unlisted beatmapset ID: {beatmapsetId}", startingBeatmapset.Id);
        }
        seedingState.IsSeeding = true;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var beatmapsets = _offset == 0
                    ? await GetRelevantBeatmapsetBatchAsync(startingBeatmapset, stoppingToken)
                    : await GetNextBeatmapsetBatchAsync(stoppingToken);
                if (beatmapsets.Count == 0)
                {
                    await FinishSeedingAsync(stoppingToken);
                    break;
                }
                
                var ids = beatmapsets.Select(bs => bs.Id).ToList();
                if (startingBeatmapset is not null && ids.Contains(startingBeatmapset.Id))
                {
                    beatmapsets = beatmapsets.Skip(ids.IndexOf(startingBeatmapset.Id) + 1).ToList();
                    startingBeatmapset = null;
                }
                logger.Log(LogLevel.Information,
                    "Processing a batch of unlisted {beatmapsetCount} beatmapsets ranked between {minDate} and {maxDate}",
                    beatmapsets.Count,
                    DateOnly.FromDateTime(beatmapsets.Min(bs => bs.RankedDate).Date),
                    DateOnly.FromDateTime(beatmapsets.Max(bs => bs.RankedDate).Date));

                using (var scope = serviceProvider.CreateScope())
                {
                    var scoreFetchingUtils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();
                    await scoreFetchingUtils.SaveAllBeatmapsetDataAsync(beatmapsets,
                        ScanEventType.SecondarySeedingStarted, stoppingToken);
                }

                foreach (var beatmapset in beatmapsets)
                {
                    using var scope = serviceProvider.CreateScope();
                    var beatmapUtils = scope.ServiceProvider.GetRequiredService<IBeatmapUtils>();
                    await beatmapUtils.ProcessBeatmapsetAsync(beatmapset, ScanEventType.SecondarySeedingStarted,
                        stoppingToken);
                }

                var beatmapsetCheckResults = await CheckIfBeatmapsetsHaveAnyScoresAsync(beatmapsets, stoppingToken);
                var removedBeatmapsets = beatmapsets.Where(bs => !beatmapsetCheckResults[bs.Id]).ToList();
                if (removedBeatmapsets.Count > 0)
                {
                    await MoveRemovedMapsetsAsync(removedBeatmapsets, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Critical, ex, "Unlisted leaderboard seeding service failed!");
                throw;
            }
        }
    }

    /// <summary>
    /// Check if a batch of <see cref="APIBeatmapset"/>s has any scores
    /// </summary>
    /// <param name="beatmapsets">The <see cref="APIBeatmapset"/>s</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>A dictionary where </returns>
    private async Task<Dictionary<int, bool>> CheckIfBeatmapsetsHaveAnyScoresAsync(IList<APIBeatmapset> beatmapsets,
        CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
        var beatmaps = beatmapsets.SelectMany(bs => bs.Beatmaps).ToList();
        var beatmapIds = beatmaps.Select(b => b.Id).ToList();
        
        var existingBeatmapIds = (await dataProcessor.GetBeatmapIdsWithScoresAsync(beatmapIds, stoppingToken)).ToHashSet();

        return beatmapsets.ToDictionary(
            bs => bs.Id, 
            bs => bs.Beatmaps.Any(b => existingBeatmapIds.Contains(b.Id)));
    }

    /// <summary>
    /// Move unlisted <see cref="Beatmapset"/>s with no scores to a removed beatmapsets table.
    /// </summary>
    /// <param name="beatmapsets">The <see cref="APIBeatmapset"/>s</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task MoveRemovedMapsetsAsync(IList<APIBeatmapset> beatmapsets, CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var beatmapsetRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetRepository>(); 
        var removedBeatmapsetRepository = scope.ServiceProvider.GetRequiredService<IRemovedBeatmapsetRepository>();
        
        var ids = beatmapsets.Select(bs => bs.Id);
        var existingBeatmapsets = await beatmapsetRepository.GetBulkAsync(ids, stoppingToken);
        if (existingBeatmapsets.Count > 0)
        {
            var currentDateTime = DateTimeOffset.Now;
            var removedBeatmapsets = existingBeatmapsets.Select(bs => new RemovedBeatmapset
            {
                Id = bs.Id,
                Artist = bs.Artist,
                Title = bs.Title,
                UserId = bs.UserId,
                Creator = bs.Creator,
                RankedDate = bs.RankedDate,
                ArchivedDate = currentDateTime
            }).ToList();
            var removedIds = removedBeatmapsets.Select(bs => bs.Id).ToList();
            var existingRemovedBeatmapsets = await removedBeatmapsetRepository.GetBulkAsync(removedIds, stoppingToken);
            var newRemovedBeatmapsets = removedBeatmapsets
                .Where(bs => !existingRemovedBeatmapsets.Select(b => b.Id).Contains(bs.Id))
                .ToList();
            if (existingRemovedBeatmapsets.Count > 0)
            {
                removedBeatmapsetRepository.UpdateBulk(existingRemovedBeatmapsets);
            }

            if (newRemovedBeatmapsets.Count > 0)
            {
                removedBeatmapsetRepository.CreateBulk(newRemovedBeatmapsets);
            }
            beatmapsetRepository.DeleteBulk(existingBeatmapsets);
            await beatmapsetRepository.SaveChangesAsync(stoppingToken);
        }
    }

    private async Task<IList<APIBeatmapset>> GetRelevantBeatmapsetBatchAsync(Beatmapset? beatmapset,
        CancellationToken stoppingToken)
    {
        var beatmapsets = await GetNextBeatmapsetBatchAsync(stoppingToken);
        if (beatmapset is null) return beatmapsets;
        var ids = beatmapsets.Select(b => b.Id).ToList();
        while (!ids.Contains(beatmapset.Id))
        {
            beatmapsets = await GetNextBeatmapsetBatchAsync(stoppingToken);
            if (beatmapsets.Count == 0) return beatmapsets;
            ids = beatmapsets.Select(b => b.Id).ToList();
        }
        return beatmapsets;
    }

    private async Task<IList<APIBeatmapset>> GetNextBeatmapsetBatchAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IDirectApiFetcher>();
        var beatmapsets = await apiFetcher.GetBeatmapsetsAsync(_offset, stoppingToken);
        _offset += beatmapsets.Length;
        return beatmapsets;
    }
    
    private async Task<Beatmapset?> GetStartingBeatmapsetAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
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
        using var scope = serviceProvider.CreateScope();
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetScanLogRepository>();
        await scanLogsRepository.SaveEventAsync(ScanEventType.SecondarySeedingFinished, stoppingToken);
        seedingState.IsSeeding = false;
        logger.Log(LogLevel.Information, "Unlisted beatmapsets seeding complete");
    }
}