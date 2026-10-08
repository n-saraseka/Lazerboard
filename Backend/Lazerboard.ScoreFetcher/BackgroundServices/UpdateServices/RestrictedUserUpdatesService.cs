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

public class RestrictedUserUpdatesService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IUserUtils _userUtils;
    private readonly ILogger<RestrictedUserUpdatesService> _logger;

    private const int BatchSize = 50;
    private readonly TimeSpan _lookbackInterval;
    private DateTime _restrictedCheckFinish;
    
    public RestrictedUserUpdatesService(IServiceProvider serviceProvider,
        IUnitOfWorkFactory unitOfWorkFactory,
        IUserUtils userUtils,
        ILogger<RestrictedUserUpdatesService> logger)
    {
        _serviceProvider = serviceProvider;
        _unitOfWorkFactory = unitOfWorkFactory;
        _userUtils = userUtils;
        _logger = logger;
        
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        var restrictedUsersUpdatehours = config.GetValue<double>("RestrictedUsersUpdateIntervalHours");
        _lookbackInterval = TimeSpan.FromHours(restrictedUsersUpdatehours);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _restrictedCheckFinish = DateTime.UtcNow - _lookbackInterval;
        while (!stoppingToken.IsCancellationRequested)
        {
            await StartUserCheckAsync(stoppingToken);
            try
            {
                var users = (await GetRestrictedUsersAsync(_restrictedCheckFinish, stoppingToken)).OrderBy(u => u.Id).ToList();
                for (var i = 0; i < users.Count; i += BatchSize)
                {
                    var batch = users.Skip(i).Take(BatchSize).ToList();
                    _logger.Log(LogLevel.Information,
                        "Processing a batch of restricted users between IDs {minId} and {maxId}",
                        batch.Min(u => u.Id), batch.Max(u => u.Id));
                    await _userUtils.ProcessRestrictedUsersAsync(batch, false, stoppingToken);
                }
                _restrictedCheckFinish = _restrictedCheckFinish.Add(_lookbackInterval);
                await FinishUserCheckAsync(stoppingToken);
                await Task.Delay(_lookbackInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "Restricted user updates service failed!");
                throw;
            }
        }
    }

    /// <summary>
    /// Get restricted <see cref="User"/>s
    /// </summary>
    /// <param name="endDate">The finishing <see cref="DateTime"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="User"/>s</returns>
    private async Task<List<User>> GetRestrictedUsersAsync(DateTime endDate, CancellationToken stoppingToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        
        return await unitOfWork.Users
            .GetRestrictedUsersAsync(endDate)
            .ToListAsync(stoppingToken);
    }
    
    private async Task StartUserCheckAsync(CancellationToken stoppingToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        
        var currentDateTime = DateTime.UtcNow;
        await unitOfWork.BeginTransactionAsync(stoppingToken);
        unitOfWork.UserScanLogs.SaveEvent(ScanEventType.RestrictedCheckStarted, currentDateTime);
        await unitOfWork.CommitTransactionAsync(stoppingToken);
        
        _logger.Log(LogLevel.Information, "Started checking restricted users at {checkStart}", currentDateTime);
    }
    
    private async Task FinishUserCheckAsync(CancellationToken stoppingToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        
        var currentDateTime = DateTime.UtcNow;
        await unitOfWork.BeginTransactionAsync(stoppingToken);
        unitOfWork.UserScanLogs.SaveEvent(ScanEventType.RestrictedCheckFinished, currentDateTime);
        await unitOfWork.CommitTransactionAsync(stoppingToken);
        
        _logger.Log(LogLevel.Information, "Finished checking restricted users at {checkStart}", currentDateTime);
    }
}