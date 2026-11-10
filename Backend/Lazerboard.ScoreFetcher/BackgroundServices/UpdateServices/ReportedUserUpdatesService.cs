using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.ScoreFetcher.Processing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.BackgroundServices.UpdateServices;

public class ReportedUserUpdatesService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReportedUserUpdatesService> _logger;
    
    private const int BatchSize = 50;
    private readonly TimeSpan _checkInterval;
    
    public ReportedUserUpdatesService(IServiceProvider serviceProvider, ILogger<ReportedUserUpdatesService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        var reportedUsersUpdateMinutes = config.GetValue<double>("ReportedUsersUpdateIntervalMinutes");
        _checkInterval = TimeSpan.FromMinutes(reportedUsersUpdateMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await StartUserCheckAsync(stoppingToken);
            try
            {
                var users = await GetReportedUsersAsync(stoppingToken);
                for (var i = 0; i < users.Count; i += BatchSize)
                {
                    var batch = users.Skip(i).Take(BatchSize).ToList();
                    _logger.Log(LogLevel.Information,
                        "Processing a batch of reported users between IDs {minId} and {maxId}",
                        batch.Min(u => u.Id), batch.Max(u => u.Id));
                    await ProcessUsersAsync(batch, stoppingToken);
                }
                await FinishUserCheckAsync(stoppingToken);
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "Reported user updates service failed!");
            }
        }
    }

    private async Task<List<User>> GetReportedUsersAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var reportsRepository = scope.ServiceProvider.GetRequiredService<IUserReportsRepository>();
        var inProgressReports = await reportsRepository.GetInProgressReportsAsync(stoppingToken);
        return inProgressReports
            .DistinctBy(r => r.ReportedUserId)
            .Select(r => r.ReportedUser)
            .OrderBy(u => u.Id)
            .ToList();
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
    
    private async Task StartUserCheckAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IUserScanLogRepository>();
        var currentDateTime = DateTime.UtcNow;
        _logger.Log(LogLevel.Information, "Started checking reported users at {checkStart}", currentDateTime);
        await scanLogsRepository.SaveEventAsync(ScanEventType.ReportedUserCheckStarted, currentDateTime, stoppingToken);
    }
    
    private async Task FinishUserCheckAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scanLogsRepository = scope.ServiceProvider.GetRequiredService<IUserScanLogRepository>();
        var currentDateTime = DateTime.UtcNow;
        _logger.Log(LogLevel.Information, "Finished checking reported users at {checkStart}", currentDateTime);
        await scanLogsRepository.SaveEventAsync(ScanEventType.ReportedUserCheckFinished, currentDateTime, stoppingToken);
    }
}