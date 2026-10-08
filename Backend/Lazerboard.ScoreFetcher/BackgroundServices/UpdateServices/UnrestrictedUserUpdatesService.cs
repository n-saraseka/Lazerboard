using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Work;
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
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IUserUtils _userUtils;
    private readonly ILogger<UnrestrictedUserUpdatesService> _logger;

    private const int BatchSize = 50;
    private readonly TimeSpan _lookbackInterval;
    private readonly TimeSpan _lookBackJitter; // For when there are scores in the previous interval inserted post-check
    private DateTime _existingCheckStart;
    private DateTime _existingCheckFinish;
    
    private DateTime? _newestScoreDate;
    private bool _shouldCatchUp;
    private bool _shouldStartCheck;
    
    public UnrestrictedUserUpdatesService(IServiceProvider serviceProvider,
        IUnitOfWorkFactory unitOfWorkFactory,
        IUserUtils userUtils,
        ILogger<UnrestrictedUserUpdatesService> logger)
    {
        _serviceProvider = serviceProvider;
        _unitOfWorkFactory = unitOfWorkFactory;
        _userUtils = userUtils;
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
            _existingCheckStart = _existingCheckStart.Add(_lookbackInterval).Subtract(_lookBackJitter);
            _existingCheckFinish = _existingCheckFinish.Add(_lookbackInterval).Add(_lookBackJitter);
            
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
                    await _userUtils.ProcessExistingUsersAsync(users, false, stoppingToken);
                }
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
        await using var unitOfWork = _unitOfWorkFactory.Create();
        
        var ids = await unitOfWork.Scores
            .GetUserIdsFromScoresAfterDate(startDate, endDate)
            .ToListAsync(stoppingToken);
        return await unitOfWork.Users.GetBulkAsync(ids, stoppingToken);
    }

    private async Task GetStartingDateTime(CancellationToken stoppingToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        
        var latestStartTimestamp = await unitOfWork.UserScanLogs.GetLatestStartedCheckAsync(stoppingToken);
        var latestFinishTimeStamp = await unitOfWork.UserScanLogs.GetLatestFinishedCheckAsync(stoppingToken);
        
        var newestScore = await unitOfWork.Scores.GetNewestScoreAsync(stoppingToken);
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
        await using var unitOfWork = _unitOfWorkFactory.Create();
        
        var currentDateTime = DateTime.UtcNow;
        await unitOfWork.BeginTransactionAsync(stoppingToken);
        unitOfWork.UserScanLogs.SaveEvent(ScanEventType.UserCheckStarted, currentDateTime);
        await unitOfWork.CommitTransactionAsync(stoppingToken);
        
        _logger.Log(LogLevel.Information, "Started checking unrestricted users at {checkStart}", currentDateTime);
    }
    
    private async Task FinishUserCheckAsync(CancellationToken stoppingToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        
        var currentDateTime = DateTime.UtcNow;
        await unitOfWork.BeginTransactionAsync(stoppingToken);
        unitOfWork.UserScanLogs.SaveEvent(ScanEventType.UserCheckFinished, currentDateTime);
        await unitOfWork.CommitTransactionAsync(stoppingToken);
        
        _logger.Log(LogLevel.Information, "Finished checking unrestricted users at {checkStart}", currentDateTime);
    }
}
