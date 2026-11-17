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
                var reports = await GetReportsAsync(stoppingToken);
                var users = reports.DistinctBy(r => r.ReportedUserId).Select(r => r.ReportedUser).ToList();
                for (var i = 0; i < users.Count; i += BatchSize)
                {
                    var batch = users.Skip(i).Take(BatchSize).ToList();
                    _logger.Log(LogLevel.Information,
                        "Processing a batch of reported users between IDs {minId} and {maxId}",
                        batch.Min(u => u.Id), batch.Max(u => u.Id));
                    var processedUsers = await ProcessUsersAsync(batch, stoppingToken);
                    var processedUserIds = processedUsers.Select(u => u.Id).ToList();
                    var relevantReports = reports.Where(r => processedUserIds.Contains(r.ReportedUserId)).ToList();
                    await UpdateReportsAsync(relevantReports, processedUsers, stoppingToken);
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

    private async Task<List<UserReport>> GetReportsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var reportsRepository = scope.ServiceProvider.GetRequiredService<IUserReportsRepository>();
        var inProgressReports = await reportsRepository.GetInProgressReportsAsync(stoppingToken);
        return inProgressReports
            .OrderBy(u => u.ReportedUserId)
            .ToList();
    }
    
    /// <summary>
    /// Process a batch of users, determine whether they are restricted or not, and process their scores
    /// </summary>
    /// <param name="users">A list of <see cref="User"/>s</param>
    /// <param name="stoppingToken">A <see cref="stoppingToken"/></param>
    private async Task<IList<User>> ProcessUsersAsync(IList<User> users, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var userUtils = scope.ServiceProvider.GetRequiredService<IUserUtils>();
        return await userUtils.ProcessExistingUsersAsync(users, false, stoppingToken);
    }

    /// <summary>
    /// Update <see cref="UserReport"/>s
    /// </summary>
    /// <param name="reports">A list of <see cref="UserReport"/>s</param>
    /// <param name="users">A list of <see cref="User"/>s</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task UpdateReportsAsync(IList<UserReport> reports, IList<User> users, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var reportsRepository = scope.ServiceProvider.GetRequiredService<IUserReportsRepository>();
        var usersHash = users.ToHashSet();
        reports = reports.Select(r =>
        {
            r.Status = usersHash.First(u => u.Id == r.ReportedUserId).IsRestricted
                ? ReportStatus.IsRestricted
                : ReportStatus.IsNotRestricted;
            return r;
        }).ToList();
        
        reportsRepository.UpdateBulk(reports);
        await reportsRepository.SaveChangesAsync(stoppingToken);

        var reportsCount = reports.Count;
        var restrictedReportsCount = reports.Count(r => r.Status == ReportStatus.IsRestricted);
        _logger.Log(LogLevel.Information, 
            "Processed {reportsCount} reports, {restrictedCount} of them reporting restricted users", 
            reportsCount, restrictedReportsCount);
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