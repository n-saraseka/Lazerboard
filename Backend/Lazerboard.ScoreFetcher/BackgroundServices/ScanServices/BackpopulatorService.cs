using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Work;
using Lazerboard.ScoreFetcher.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.BackgroundServices.ScanServices;

public class BackpopulatorService(IServiceProvider serviceProvider,
    IUnitOfWorkFactory unitOfWorkFactory,
    IDataProcessor dataProcessor,
    ILogger<BackpopulatorService> logger) : BackgroundService
{
    private const int BatchSize = 15;
    private const int DelayBetweenBatches = 500;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var startedAt = DateTime.UtcNow;
        try
        {
            while (await AddMissingUserAttributesToBeatmapsetsAsync(stoppingToken))
            {
                await Task.Delay(DelayBetweenBatches, stoppingToken);
            }
            var elapsed = DateTime.UtcNow - startedAt;
            logger.Log(LogLevel.Information, "Added missing user attributes in {executionTime}", elapsed);
        
            startedAt = DateTime.UtcNow;
            while (await AddMissingHealthAttributesAsync(stoppingToken))
            {
                await Task.Delay(DelayBetweenBatches, stoppingToken);
            }
            elapsed = DateTime.UtcNow - startedAt;
            logger.Log(LogLevel.Information, "Added missing health attributes in {executionTime}", elapsed);

            startedAt = DateTime.UtcNow;
            while (await AddMissingConvertFlagsAsync(stoppingToken))
            {
                await Task.Delay(DelayBetweenBatches, stoppingToken);
            }
            elapsed = DateTime.UtcNow - startedAt;
            logger.Log(LogLevel.Information, "Added missing convert flags in {executionTime}", elapsed);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        { }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Critical, ex, "Backpopulator service failed!");
        }
    }
    
    /// <summary>
    /// Add missing <see cref="Beatmapset.UserId"/>s
    /// </summary>
    /// <param name="token">A <see cref="CancellationToken"/></param>
    /// <returns>True if should continue backpopulation, false otherwise</returns>
    private async Task<bool> AddMissingUserAttributesToBeatmapsetsAsync(CancellationToken token)
    {
        using var scope = serviceProvider.CreateScope();
        var unitOfWork = unitOfWorkFactory.Create();
        var beatmapsets = await unitOfWork.Beatmapsets
            .GetAll()
            .Where(b => b.UserId == null)
            .Take(BatchSize)
            .ToListAsync(token);
        if (beatmapsets.Count == 0) return false;
        
        logger.Log(LogLevel.Information, "Adding missing user ID attributes to {beatmapsetCount} beatmapsets", beatmapsets.Count);
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IOsuApiFetcher>();
        
        var beatmapsetIds = beatmapsets.Select(b => b.Id).ToList();
        var beatmaps = await unitOfWork.Beatmaps.GetAll().Where(b => beatmapsetIds.Contains(b.BeatmapsetId)).ToListAsync(token);
        
        var apiBeatmaps = await apiFetcher.GetBeatmapsAsync(beatmaps.Select(b => b.Id).ToList(), token);
        var apiBeatmapsets = apiBeatmaps.Select(b => b.Beatmapset).DistinctBy(b => b.Id).ToList();
        
        var userIds = apiBeatmapsets.Select(b => b.UserId).Distinct().ToList();
        var apiUsers = await apiFetcher.GetUsersAsync(userIds, token);
        var apiCountries = apiUsers.Select(u => u.Country).DistinctBy(c => c.Code).ToList();
        await dataProcessor.ProcessCountriesAsync(apiCountries, token);
        await dataProcessor.ProcessUsersAsync(apiUsers, token);
        
        var apiUserIds = apiUsers.Select(u => u.Id).ToList();
        var deletedOrRestrictedUserIds = userIds.Where(id => !apiUserIds.Contains(id)).ToList();

        var deletedOrRestrictedUsers = deletedOrRestrictedUserIds.Select(id => new User
        {
            Id = id,
            Username = apiBeatmapsets.First(b => b.UserId == id).Creator
        });
        
        var existingUsers = await unitOfWork.Users.GetBulkAsync(deletedOrRestrictedUserIds, token);
        var existingUserIds = existingUsers.Select(u => u.Id).ToList();
        var newUsers = deletedOrRestrictedUsers.Where(u => !existingUserIds.Contains(u.Id));
        
        await unitOfWork.BeginTransactionAsync(token);
        unitOfWork.Users.CreateBulk(newUsers);
        await unitOfWork.CommitTransactionAsync(token);
        
        await unitOfWork.BeginTransactionAsync(token);
        foreach (var beatmapset in beatmapsets)
        {
            var respectiveApiBeatmapset = apiBeatmapsets.FirstOrDefault(b => b.Id == beatmapset.Id);
            beatmapset.Creator = respectiveApiBeatmapset?.Creator;
            beatmapset.UserId = respectiveApiBeatmapset?.UserId ?? 0;
            unitOfWork.Beatmapsets.Update(beatmapset);
        }
        await unitOfWork.CommitTransactionAsync(token);

        return true;
    }
    
    /// <summary>
    /// Add missing <see cref="Beatmap.Health"/> attributes
    /// </summary>
    /// <param name="token">A <see cref="CancellationToken"/></param>
    /// <returns>True if should continue backpopulation, false otherwise</returns>
    private async Task<bool> AddMissingHealthAttributesAsync(CancellationToken token)
    {
        using var scope = serviceProvider.CreateScope();
        var unitOfWork = unitOfWorkFactory.Create();
        
        var beatmaps = await unitOfWork.Beatmaps
            .GetAll()
            .Where(b => b.Health == null)
            .Take(BatchSize)
            .ToListAsync(token);

        if (beatmaps.Count == 0) return false;
        
        logger.Log(LogLevel.Information, "Adding missing health attributes to {beatmapCount} beatmaps", beatmaps.Count);
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IOsuApiFetcher>();
        var apiBeatmaps = await apiFetcher.GetBeatmapsAsync(beatmaps.Select(b => b.Id).ToList(), token);

        await unitOfWork.BeginTransactionAsync(token);
        foreach (var beatmap in beatmaps)
        {
            var respectiveApiBeatmap = apiBeatmaps.FirstOrDefault(b => b.Id == beatmap.Id);
            beatmap.Health = respectiveApiBeatmap?.Health ?? 0;
            beatmap.DrainLength = respectiveApiBeatmap?.DrainLength ?? 0;
            unitOfWork.Beatmaps.Update(beatmap);
        }

        await unitOfWork.CommitTransactionAsync(token);

        return true;
    }

    private async Task<bool> AddMissingConvertFlagsAsync(CancellationToken token)
    {
        await using var unitOfWork = unitOfWorkFactory.Create();
        
        // Filter beatmaps to ones that have any scores with the null convert flag.
        var mapsWithNullConvertFlags = unitOfWork.Beatmaps
            .GetAll()
            .Where(b => b.Scores.Any(s => s.IsConvert == null));

        var batch = await mapsWithNullConvertFlags
            .Take(BatchSize)
            .ToDictionaryAsync(b => b.Id, b => b.Mode, token);
        if (batch.Count == 0) return false;
        
        logger.Log(LogLevel.Information, "Adding missing convert attributes to {beatmapCount} beatmaps", batch.Count);

        await unitOfWork.BeginTransactionAsync(token);
        var updatedScores = await unitOfWork.Scores
            .GetDbContext()
            .Database
            .ExecuteSqlAsync($"UPDATE scores s SET is_convert = (s.mode != v.mode) FROM unnest({batch.Select(kvp => kvp.Key)}::int[], {batch.Select(kvp => kvp.Value)}::mode[]) AS v(beatmap_id, mode) WHERE s.beatmap_id = v.beatmap_id", 
                token);
        logger.Log(LogLevel.Information, "Added missing convert attributes to {scoreCount} scores", updatedScores);
        return true;
    }
}