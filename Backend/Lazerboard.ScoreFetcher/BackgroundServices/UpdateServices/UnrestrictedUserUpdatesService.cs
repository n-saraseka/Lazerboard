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

public class UnrestrictedUserUpdatesService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UnrestrictedUserUpdatesService> _logger;

    private const int BatchSize = 50;
    private readonly TimeSpan _lookbackInterval;
    private readonly TimeSpan _lookBackJitter; // For when there are scores in the previous interval inserted post-check
    private DateTime _existingCheckStart;
    private DateTime _existingCheckFinish;
    
    private DateTime? _newestScoreDate;
    private bool _shouldCatchUp;
    private bool _shouldStartCheck;
    
    public UnrestrictedUserUpdatesService(IServiceProvider serviceProvider, ILogger<UnrestrictedUserUpdatesService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var existingUsersUpdateHours = config.GetValue<double>("UserUpdatesIntervalHours");
        _lookbackInterval = TimeSpan.FromHours(existingUsersUpdateHours);
        _lookBackJitter = TimeSpan.FromMinutes(45);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await GetStartingDateTime(stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            _shouldCatchUp = _newestScoreDate != null && _newestScoreDate - _existingCheckFinish  > _lookbackInterval;
            if (_shouldStartCheck)
            {
                await StartUserCheckAsync(stoppingToken);
                _shouldStartCheck = false;
            }
            try
            {
                var users = await GetLatestUsersAsync(_existingCheckStart, _existingCheckFinish, stoppingToken);
                for (var i = 0; i < users.Count; i += BatchSize)
                {
                    var batch = users.Skip(i).Take(BatchSize).ToList();
                    _logger.Log(LogLevel.Information,
                        "Processing a batch of existing users between IDs {minId} and {maxId}",
                        batch.Min(u => u.Id), batch.Max(u => u.Id));
                    await ProcessUsersAsync(batch, stoppingToken);
                }
                _existingCheckStart = _existingCheckStart.Add(_lookbackInterval).Subtract(_lookBackJitter);
                _existingCheckFinish = _existingCheckFinish.Add(_lookbackInterval).Add(_lookBackJitter);
                if (!_shouldCatchUp)
                {
                    await FinishUserCheckAsync(stoppingToken);
                    _shouldStartCheck = true;
                    await Task.Delay(_lookbackInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "Unrestricted user updates service failed!");
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
    private async Task<List<User>> GetLatestUsersAsync(DateTime startDate, DateTime endDate, CancellationToken stoppingToken)
    {
        _logger.Log(LogLevel.Information, "Getting unrestricted users from scores between {startDate} and {endDate}", startDate, endDate);
        using var scope = _serviceProvider.CreateScope();
        var scoreRepository = scope.ServiceProvider.GetRequiredService<IScoreRepository>();
        var ids = await scoreRepository
            .GetUserIdsFromScoresAfterDate(startDate, endDate)
            .ToListAsync(stoppingToken);
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        return await userRepository.GetBulkAsync(ids, stoppingToken);
    }
    
    /// <summary>
    /// Process a batch of users, determine whether they are restricted or not, and process their scores
    /// </summary>
    /// <param name="users">A list of <see cref="User"/>s</param>
    /// <param name="stoppingToken">A <see cref="stoppingToken"/></param>
    private async Task ProcessUsersAsync(IList<User> users, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var userUtils = scope.ServiceProvider.GetRequiredService<IUserUtils>();
        await userUtils.ProcessExistingUsersAsync(users, false, stoppingToken);
    }

    private async Task GetStartingDateTime(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IUserScanLogRepository>();
        var latestStartTimestamp = await scanLogsRepository.GetLatestStartedCheckAsync(stoppingToken);
        var latestFinishTimeStamp = await scanLogsRepository.GetLatestFinishedCheckAsync(stoppingToken);
        
        var scoreRepository = scope.ServiceProvider.GetRequiredService<IScoreRepository>();
        var newestScore = await scoreRepository.GetNewestScoreAsync(stoppingToken);
        _newestScoreDate = newestScore?.Date;

        if (latestStartTimestamp is null)
        {
            _existingCheckFinish = newestScore?.Date ?? DateTime.UtcNow;
            _shouldStartCheck = true;
        }
        else
        {
            _shouldStartCheck = latestFinishTimeStamp != null
                                && latestFinishTimeStamp.LoggedAt > latestStartTimestamp.LoggedAt;
            _existingCheckFinish = latestStartTimestamp.LoggedAt;
            if (_shouldStartCheck)
            {
                var currentDateTime = DateTime.UtcNow;
                _existingCheckFinish = currentDateTime - _existingCheckFinish < _lookbackInterval 
                    ? currentDateTime 
                    : _existingCheckFinish.Add(_lookbackInterval);
            }
        }
        _existingCheckStart = _existingCheckFinish.Subtract(_lookbackInterval);
    }

    private async Task StartUserCheckAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IUserScanLogRepository>();
        var currentDateTime = DateTime.UtcNow;
        _logger.Log(LogLevel.Information, "Started checking unrestricted users at {checkStart}", currentDateTime);
        await scanLogsRepository.SaveEventAsync(ScanEventType.UserCheckStarted, currentDateTime, stoppingToken);
    }
    
    private async Task FinishUserCheckAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IUserScanLogRepository>();
        var currentDateTime = DateTime.UtcNow;
        _logger.Log(LogLevel.Information, "Finished checking unrestricted users at {checkStart}", currentDateTime);
        await scanLogsRepository.SaveEventAsync(ScanEventType.UserCheckFinished, currentDateTime, stoppingToken);
    }
}
