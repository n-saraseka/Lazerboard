using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.ScoreFetcher.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.BackgroundServices.ScanServices;

public class UnlistedUserScanService(
    IServiceProvider serviceProvider,
    ILogger<UnlistedUserScanService> logger) : BackgroundService
{
    private const int BatchSize = 50;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await StartScanningAsync(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var users = await GetUsersAsync(stoppingToken);

                for (var i = 0; i < users.Count; i += BatchSize)
                {
                    var batch = users.Skip(i).Take(BatchSize).ToList();
                    logger.Log(LogLevel.Information, 
                        "Processing a batch of {userCount} unlisted users between IDs {minId} and {maxId}", 
                        batch.Count, batch.Min(u => u.Id), batch.Max(u => u.Id));
                    
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
        await userUtils.ProcessRestrictedUsersAsync(users, stoppingToken);
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
    
    private async Task StartScanningAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IUserScanLogRepository>();
        var latestStartTimestamp = await scanLogsRepository.GetLatestStartedScanAsync(stoppingToken);
        var latestFinishTimeStamp = await scanLogsRepository.GetLatestFinishedScanAsync(stoppingToken);
            
        if (latestStartTimestamp is null 
            || (latestFinishTimeStamp != null && latestFinishTimeStamp.LoggedAt > latestStartTimestamp.LoggedAt))
        {
            var currentDateTime = DateTime.UtcNow;
            await scanLogsRepository.SaveEventAsync(ScanEventType.UserScanStarted, currentDateTime, stoppingToken);
            logger.Log(LogLevel.Information, "Started scanning users at {datetime}", DateTime.UtcNow);
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