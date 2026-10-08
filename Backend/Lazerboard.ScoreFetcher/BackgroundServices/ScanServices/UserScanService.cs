using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.Database.Work;
using Lazerboard.ScoreFetcher.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.BackgroundServices.ScanServices;

public class UserScanService(IUnitOfWorkFactory unitOfWorkFactory,
    IUserUtils userUtils,
    IServiceProvider serviceProvider,
    ILogger<UserScanService> logger) : BackgroundService
{
    private const int BatchSize = 50;
    private int? _latestUserId;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await GetStartingDataAsync(stoppingToken);
        logger.Log(LogLevel.Information, "Starting user ID: {startingUserId}", _latestUserId);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var users = await GetUsersAsync(stoppingToken);

                if (users.Count == 0)
                {
                    await FinishScanningAsync(stoppingToken);
                    break;
                }
                
                logger.Log(LogLevel.Information, 
                    "Processing a batch of {userCount} users between IDs {minId} and {maxId}", 
                    users.Count, users.Min(u => u.Id), users.Max(u => u.Id));

                await userUtils.ProcessExistingUsersAsync(users, true, stoppingToken);
                
                var latestUser = users.MaxBy(u => u.Id);
                if (latestUser is not null)
                {
                    _latestUserId = latestUser.Id;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Critical, ex, "User scan service failed!");
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
        var userId = _latestUserId ?? 0;
        return await unitOfWork.Users.GetAll()
            .Where(u => u.Id > userId)
            .OrderBy(u => u.Id)
            .Take(BatchSize)
            .AsNoTracking()
            .ToListAsync(stoppingToken);
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
            
            logger.Log(LogLevel.Information, "Started scanning users at {datetime}", DateTime.UtcNow);
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