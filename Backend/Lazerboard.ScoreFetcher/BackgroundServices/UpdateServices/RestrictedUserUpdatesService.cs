using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
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
    private readonly ILogger<RestrictedUserUpdatesService> _logger;

    private const int BatchSize = 50;
    private readonly TimeSpan _lookbackInterval;
    private DateTime _restrictedCheckFinish;
    
    public RestrictedUserUpdatesService(IServiceProvider serviceProvider, ILogger<RestrictedUserUpdatesService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        var restrictedUsersUpdatehours = config.GetValue<double>("RestrictedUsersUpdateIntervalHours");
        _lookbackInterval = TimeSpan.FromHours(restrictedUsersUpdatehours);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _restrictedCheckFinish = DateTime.UtcNow;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var users = (await GetRestrictedUsersAsync(_restrictedCheckFinish, stoppingToken)).OrderBy(u => u.Id).ToList();
                for (var i = 0; i < users.Count; i += BatchSize)
                {
                    var batch = users.Skip(i).Take(BatchSize).ToList();
                    _logger.Log(LogLevel.Information,
                        "Processing a batch of restricted users between IDs {minId} and {maxId}",
                        batch.Min(u => u.Id), batch.Max(u => u.Id));
                    await ProcessRestrictedUsersAsync(batch, stoppingToken);
                }
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
        using var scope = _serviceProvider.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        return await userRepository
            .GetRestrictedUsersAsync(endDate)
            .ToListAsync(stoppingToken);
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
}