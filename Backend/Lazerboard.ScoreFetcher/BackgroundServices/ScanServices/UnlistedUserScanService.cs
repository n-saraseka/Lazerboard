using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Work;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.ScoreFetcher.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.BackgroundServices.ScanServices;

public class UnlistedUserScanService(IUnitOfWorkFactory unitOfWorkFactory,
    IBeatmapUtils beatmapUtils,
    IUserUtils userUtils,
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
                            await beatmapUtils.ProcessLeaderboardAsync(beatmapId, mode, stoppingToken);
                        }
                    }
                    
                    await userUtils.ProcessRestrictedUsersAsync(users, true, stoppingToken);
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
    /// Get a batch of <see cref="User"/>s from the database
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="User"/>s</returns>
    private async Task<List<User>> GetUsersAsync(CancellationToken stoppingToken)
    {
        await using var unitOfWork = unitOfWorkFactory.Create();
        var userIds = await unitOfWork.UnlistedScores.GetAllUsersAsync(stoppingToken);
        return await unitOfWork.Users.GetBulkAsync(userIds, stoppingToken);
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
        await using var unitOfWork = unitOfWorkFactory.Create();
        var scores = await unitOfWork.UnlistedScores.GetByUserIds(userIds).ToListAsync(stoppingToken);
        return scores
            .GroupBy(s => s.BeatmapId)
            .ToDictionary(g => g.Key, g => g.Select(s => s.Mode).Distinct().ToList());
    }
    
    private async Task GetStartingDataAsync(CancellationToken stoppingToken)
    {
        await using var unitOfWork = unitOfWorkFactory.Create();
        
        var latestStartTimestamp = await unitOfWork.UserScanLogs.GetLatestStartedScanAsync(stoppingToken);
        var latestFinishTimeStamp = await unitOfWork.UserScanLogs.GetLatestFinishedScanAsync(stoppingToken);
            
        if (latestStartTimestamp is null 
            || (latestFinishTimeStamp != null && latestFinishTimeStamp.LoggedAt > latestStartTimestamp.LoggedAt))
        {
            var currentDateTime = DateTime.UtcNow;
            
            await unitOfWork.BeginTransactionAsync(stoppingToken);
            unitOfWork.UserScanLogs.SaveEvent(ScanEventType.UserScanStarted, currentDateTime);
            await unitOfWork.CommitTransactionAsync(stoppingToken);
            
            logger.Log(LogLevel.Information, "Started scanning unlisted users at {datetime}", DateTime.UtcNow);
            return;
        }
        
        var latestScannedUser = await unitOfWork.Users.GetLatestScannedUserAsync(stoppingToken);
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
        await using var unitOfWork = unitOfWorkFactory.Create();
        
        var currentDateTime = DateTime.UtcNow;
        
        await unitOfWork.BeginTransactionAsync(stoppingToken);
        unitOfWork.UserScanLogs.SaveEvent(ScanEventType.UserScanStarted, currentDateTime);
        await unitOfWork.CommitTransactionAsync(stoppingToken);
        
        logger.Log(LogLevel.Information, "User scan complete");
    }
}