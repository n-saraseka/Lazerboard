using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.ScoreFetcher.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.BackgroundServices.ScanServices;

public class UserScanService(
    IServiceProvider serviceProvider,
    ILogger<UserScanService> logger) : BackgroundService
{
    private const int BatchSize = 500;
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

                await ProcessUsersAsync(users, stoppingToken);
                
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
    /// Process a batch of users, determine whether they are restricted or not, and process their scores
    /// </summary>
    /// <param name="users">A list of <see cref="User"/>s</param>
    /// <param name="stoppingToken">A <see cref="stoppingToken"/></param>
    private async Task ProcessUsersAsync(IList<User> users, CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var userUtils = scope.ServiceProvider.GetRequiredService<IUserUtils>();
        await userUtils.ProcessUsersAsync(users, stoppingToken);
    }
    
    /// <summary>
    /// Get a batch of <see cref="User"/>s from the database
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="User"/>s</returns>
    private async Task<List<User>> GetUsersAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        return await userRepository.GetAll()
            .Where(u => u.Id > _latestUserId)
            .OrderBy(u => u.Id)
            .Take(BatchSize)
            .AsNoTracking()
            .ToListAsync(stoppingToken);
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
            await scanLogsRepository.SaveEventAsync(ScanEventType.UserScanStarted, stoppingToken);
            logger.Log(LogLevel.Information, "Started scanning users at {datetime}", DateTime.UtcNow);
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
        await scanLogsRepository.SaveEventAsync(ScanEventType.UserScanFinished, stoppingToken);
        logger.Log(LogLevel.Information, "User scan complete");
    }
}