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
    private readonly TimeSpan _lookBackJitter; // For when there are scores in the previous interval inserted post-check
    private DateTime _existingCheckStart;
    private DateTime _existingCheckFinish;
    private DateTime _restrictedCheckFinish;
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
        _lookBackJitter = TimeSpan.FromMinutes(45);
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
            try
            {
                var userIds = await GetLatestUserIdsAsync(_existingCheckStart, _existingCheckFinish, stoppingToken);
                var users = (await GetUsersAsync(userIds, stoppingToken)).OrderBy(u => u.Id).ToList(); 
                for (var i = 0; i < users.Count; i += BatchSize)
                {
                    var batch = users.Skip(i).Take(BatchSize).ToList();
                    _logger.Log(LogLevel.Information,
                        "Processing a batch of existing users between IDs {minId} and {maxId}",
                        batch.Min(u => u.Id), batch.Max(u => u.Id));
                    await ProcessExistingUsersAsync(batch, stoppingToken);
                }
                
                users = (await GetRestrictedUsersAsync(_restrictedCheckFinish, stoppingToken)).OrderBy(u => u.Id).ToList();
                for (var i = 0; i < users.Count; i += BatchSize)
                {
                    var batch = users.Skip(i).Take(BatchSize).ToList();
                    _logger.Log(LogLevel.Information,
                        "Processing a batch of restricted users between IDs {minId} and {maxId}",
                        batch.Min(u => u.Id), batch.Max(u => u.Id));
                    await ProcessRestrictedUsersAsync(batch, stoppingToken);
                }
                
                _existingCheckStart = _existingCheckStart.Add(_existingUsersLookbackInterval).Subtract(_lookBackJitter);
                _existingCheckFinish = _existingCheckFinish.Add(_existingUsersLookbackInterval).Add(_lookBackJitter);
                _restrictedCheckFinish = _restrictedCheckFinish.Add(_restrictedUsersLookbackInterval);
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
    /// Get <see cref="User.Id"/>s from recent scores
    /// </summary>
    /// <param name="startDate">The starting <see cref="DateTime"/></param>
    /// <param name="endDate">The finishing <see cref="DateTime"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="User"/>s</returns>
    private async Task<List<int>> GetLatestUserIdsAsync(DateTime startDate, DateTime endDate, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scoreRepository = scope.ServiceProvider.GetRequiredService<IScoreRepository>();
        return await scoreRepository
            .GetUserIdsFromScoresAfterDate(startDate, endDate)
            .ToListAsync(stoppingToken);
    }

    /// <summary>
    /// Get restricted <see cref="User"/>s
    /// </summary>
    /// <param name="endDate">The finishing <see cref="DateTime"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="User"/>s</returns>
    private async Task<List<User>> GetRestrictedUsersAsync(DateTime endDate, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        return await userRepository
            .GetRestrictedUsersAsync(endDate)
            .ToListAsync(stoppingToken);
    }

    /// <summary>
    /// Get <see cref="User"/>s from the DB
    /// </summary>
    /// <param name="userIds">List of <see cref="User"/> IDs</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="User"/>s</returns>
    private async Task<List<User>> GetUsersAsync(IList<int> userIds, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        return await userRepository.GetBulkAsync(userIds, stoppingToken);
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
        _restrictedCheckFinish = DateTime.UtcNow;
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
