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

public class UnlistedUserScanService(
    IServiceProvider serviceProvider,
    ILogger<UnlistedUserScanService> logger) : BackgroundService
{
    private const int BatchSize = 10;
    private int? _latestUserId;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await GetStartingDataAsync(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var users = (await GetUsersAsync(stoppingToken)).OrderBy(u => u.Id).ToList();
                if (_latestUserId.HasValue)
                {
                    users = users.Where(u => u.Id > _latestUserId.Value).ToList();
                }

                for (var i = 0; i < users.Count; i += BatchSize)
                {
                    var batch = users.Skip(i).Take(BatchSize).ToList();
                    var userIds = batch.Select(u => u.Id).ToList();
                    logger.Log(LogLevel.Information,
                        "Processing a batch of {userCount} users from unlisted scores between IDs {minId} and {maxId}", 
                        batch.Count, batch.Min(u => u.Id), batch.Max(u => u.Id));

                    var leaderboards = await GetLeaderboardsFromUsersAsync(userIds, stoppingToken);
                    foreach (var beatmapId in leaderboards.Keys)
                    {
                        foreach (var mode in leaderboards[beatmapId])
                        {
                            await ProcessLeaderboardAsync(beatmapId, mode, stoppingToken);
                        }
                    }
                    
                    await ProcessUsersAsync(users, stoppingToken);
                }
                await FinishScanningAsync(stoppingToken);
                break;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Critical, ex, "Unlisted user scan service failed!");
                throw;
            }
        }
    }

    /// <summary>
    /// Process a batch of users, determine whether they are restricted or not, and process their scores
    /// </summary>
    /// <param name="users">A list of <see cref="User"/>s</param>
    /// <param name="stoppingToken">A <see cref="stoppingToken"/></param>
    private async Task ProcessUsersAsync(IList<User> users, CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var userUtils = scope.ServiceProvider.GetRequiredService<IUserUtils>();
        await userUtils.ProcessRestrictedUsersAsync(users, true, stoppingToken);
    }
    
    /// <summary>
    /// Get a batch of <see cref="User"/>s from the database
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="User"/>s</returns>
    private async Task<List<User>> GetUsersAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var unlistedScoreRepository = scope.ServiceProvider.GetRequiredService<IUnlistedScoreRepository>();
        var userIds = await unlistedScoreRepository.GetAllUsersAsync(stoppingToken);
        
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        return await userRepository.GetBulkAsync(userIds, stoppingToken);
    }

    /// <summary>
    /// Get leaderboards from user scores
    /// </summary>
    /// <param name="userIds">List of <see cref="User"/> IDs</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>A dictionary of <see cref="Beatmap"/> IDs and respective <see cref="Mode"/>s</returns>
    private async Task<Dictionary<int, List<Mode>>> GetLeaderboardsFromUsersAsync(IList<int> userIds,
        CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var unlistedScoreRepository = scope.ServiceProvider.GetRequiredService<IUnlistedScoreRepository>();
        var scores = await unlistedScoreRepository.GetByUserIds(userIds).ToListAsync(stoppingToken);
        return scores
            .GroupBy(s => s.BeatmapId)
            .ToDictionary(g => g.Key, g => g.Select(s => s.Mode).Distinct().ToList());
    }

    /// <summary>
    /// Process a <see cref="Beatmap"/> leaderboard
    /// </summary>
    /// <param name="beatmapId">The <see cref="Beatmap.Id"/></param>
    /// <param name="mode">The <see cref="Score"/>.Mode</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task ProcessLeaderboardAsync(int beatmapId, Mode mode, CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var beatmapUtils = scope.ServiceProvider.GetRequiredService<IBeatmapUtils>();

        await beatmapUtils.ProcessLeaderboardAsync(beatmapId, mode, stoppingToken);
    }
    
    private async Task GetStartingDataAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IUserScanLogRepository>();
        var latestStartTimestamp = await scanLogsRepository.GetLatestStartedScanAsync(stoppingToken);
        var latestFinishTimeStamp = await scanLogsRepository.GetLatestFinishedScanAsync(stoppingToken);
            
        if (latestStartTimestamp is null 
            || (latestFinishTimeStamp != null && latestFinishTimeStamp.LoggedAt > latestStartTimestamp.LoggedAt))
        {
            var currentDateTime = DateTime.UtcNow;
            await scanLogsRepository.SaveEventAsync(ScanEventType.UserScanStarted, currentDateTime, stoppingToken);
            logger.Log(LogLevel.Information, "Started scanning unlisted users at {datetime}", DateTime.UtcNow);
            return;
        }
        
        var latestScannedUser = await userRepository.GetLatestScannedUserAsync(stoppingToken);
        if (latestScannedUser != null)
        {
            _latestUserId  = latestScannedUser.Id;
        }
    }
    
    /// <summary>
    /// Save the <see cref="ScanEventType.UserScanFinished"/> event
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task FinishScanningAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IUserScanLogRepository>();
        var currentDateTime = DateTime.UtcNow;
        await scanLogsRepository.SaveEventAsync(ScanEventType.UserScanFinished, currentDateTime, stoppingToken);
        logger.Log(LogLevel.Information, "User scan complete");
    }
}