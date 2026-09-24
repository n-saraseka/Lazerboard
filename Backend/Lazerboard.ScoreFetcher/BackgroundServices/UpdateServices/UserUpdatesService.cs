using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.ScoreFetcher.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.BackgroundServices.UpdateServices;

public class UserUpdatesService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserUpdatesService> _logger;

    private const int BatchSize = 50;
    private readonly TimeSpan _existingUsersLookbackInterval;
    private readonly TimeSpan _restrictedUsersLookbackInterval;
    private DateTime _existingCheckStart;
    private DateTime _existingCheckFinish;
    private bool _shouldStartCheck;
    
    public UserUpdatesService(IServiceProvider serviceProvider, ILogger<UserUpdatesService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var existingUsersUpdateHours = config.GetValue<double>("UserUpdatesIntervalHours");
        var restrictedUsersUpdatehours = config.GetValue<double>("RestrictedUsersUpdateIntervalHours");
        _existingUsersLookbackInterval = TimeSpan.FromHours(existingUsersUpdateHours);
        _restrictedUsersLookbackInterval = TimeSpan.FromHours(restrictedUsersUpdatehours);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await GetStartingDateTime(stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_shouldStartCheck)
            {
                await StartUserCheckAsync(stoppingToken);
                _shouldStartCheck = false;
            }
            var currentDateTime = DateTime.UtcNow;
            try
            {
                var users = await GetLatestUsersAsync(_existingCheckStart, _existingCheckFinish, 0, stoppingToken);
                for (var i = 1; users.Count > 0; i++)
                {
                    await ProcessExistingUsersAsync(users, stoppingToken);
                    users = await GetLatestUsersAsync(_existingCheckStart, _existingCheckFinish, i, stoppingToken);
                }

                var restrictedUsers = await GetRestrictedUsersAsync(currentDateTime, 0, stoppingToken);
                for (var i = 1; restrictedUsers.Count > 0; i++)
                {
                    await ProcessRestrictedUsersAsync(restrictedUsers, stoppingToken);
                    restrictedUsers = await GetRestrictedUsersAsync(currentDateTime, i, stoppingToken);
                }
                
                _existingCheckStart = _existingCheckStart.Add(_existingUsersLookbackInterval);
                _existingCheckFinish = _existingCheckFinish.Add(_restrictedUsersLookbackInterval);
                await FinishUserCheckAsync(stoppingToken);
                _shouldStartCheck = true;
                await Task.Delay(_existingUsersLookbackInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "User updates service failed!");
                throw;
            }
        }
    }
    
    /// <summary>
    /// Get a batch of <see cref="User"/>s from recent scores
    /// </summary>
    /// <param name="startDate">The starting <see cref="DateTime"/></param>
    /// <param name="endDate">The finishing <see cref="DateTime"/></param>
    /// <param name="batchNumber">The batch number</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="User"/>s</returns>
    private async Task<List<User>> GetLatestUsersAsync(DateTime startDate, DateTime endDate, int batchNumber, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scoreRepository = scope.ServiceProvider.GetRequiredService<IScoreRepository>();
        return await scoreRepository
            .GetUsersFromScoresAfterDate(startDate, endDate)
            .Skip(batchNumber * BatchSize)
            .Take(BatchSize)
            .ToListAsync(stoppingToken);
    }

    /// <summary>
    /// Get a batch of restricted <see cref="User"/>s
    /// </summary>
    /// <param name="startDate">The starting <see cref="DateTime"/></param>
    /// <param name="batchNumber">The batch number</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="User"/>s</returns>
    private async Task<List<User>> GetRestrictedUsersAsync(DateTime startDate, int batchNumber,
        CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        return await userRepository
            .GetRestrictedUsersAsync(startDate, _restrictedUsersLookbackInterval)
            .Skip(batchNumber * BatchSize)
            .Take(BatchSize)
            .ToListAsync(stoppingToken);
    }
    
    /// <summary>
    /// Process a batch of users, determine whether they are restricted or not, and process their scores
    /// </summary>
    /// <param name="users">A list of <see cref="User"/>s</param>
    /// <param name="stoppingToken">A <see cref="stoppingToken"/></param>
    private async Task ProcessExistingUsersAsync(IList<User> users, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var userUtils = scope.ServiceProvider.GetRequiredService<IUserUtils>();
        await userUtils.ProcessExistingUsersAsync(users, false, stoppingToken);
    }
    
    /// <summary>
    /// Process a batch of users, determine whether they are still restricted or not, and process their scores
    /// </summary>
    /// <param name="users">A list of <see cref="User"/>s</param>
    /// <param name="stoppingToken">A <see cref="stoppingToken"/></param>
    private async Task ProcessRestrictedUsersAsync(IList<User> users, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var userUtils = scope.ServiceProvider.GetRequiredService<IUserUtils>();
        await userUtils.ProcessRestrictedUsersAsync(users, stoppingToken);
    }

    private async Task GetStartingDateTime(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IUserScanLogRepository>();
        var latestStartTimestamp = await scanLogsRepository.GetLatestStartedCheckAsync(stoppingToken);
        var latestFinishTimeStamp = await scanLogsRepository.GetLatestFinishedCheckAsync(stoppingToken);

        if (latestStartTimestamp is null)
        {
            var scoreRepository = scope.ServiceProvider.GetRequiredService<IScoreRepository>();
            var newestScore = await scoreRepository.GetNewestScoreAsync(stoppingToken);
            _existingCheckFinish = newestScore?.Date ?? DateTime.UtcNow;
            _shouldStartCheck = true;
        }
        else
        {
            if (latestFinishTimeStamp != null
                && latestFinishTimeStamp.LoggedAt > latestStartTimestamp.LoggedAt)
            {
                _shouldStartCheck = true;
                _existingCheckFinish = latestStartTimestamp.LoggedAt.Add(_existingUsersLookbackInterval);
            }
            else
            {
                _existingCheckFinish = latestStartTimestamp.LoggedAt;
            }
        }
        _existingCheckStart = _existingCheckFinish.Subtract(_existingUsersLookbackInterval);
    }

    private async Task StartUserCheckAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IUserScanLogRepository>();
        var currentDateTime = DateTime.UtcNow;
        _logger.Log(LogLevel.Information, "Started checking users at {checkStart}", currentDateTime);
        await scanLogsRepository.SaveEventAsync(ScanEventType.UserCheckStarted, currentDateTime, stoppingToken);
    }
    
    private async Task FinishUserCheckAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IUserScanLogRepository>();
        var currentDateTime = DateTime.UtcNow;
        _logger.Log(LogLevel.Information, "Finished checking users at {checkStart}", currentDateTime);
        await scanLogsRepository.SaveEventAsync(ScanEventType.UserCheckFinished, currentDateTime, stoppingToken);
    }
}
