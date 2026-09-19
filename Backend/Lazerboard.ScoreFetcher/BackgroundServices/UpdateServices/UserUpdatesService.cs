using Lazerboard.Data.Database.Entities;
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
    private DateTime _startingDateTime;
    
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
        _startingDateTime = DateTime.UtcNow;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var users = await GetLatestUsersAsync(_startingDateTime, 0, stoppingToken);
                for (var i = 1; users.Count > 0; i++)
                {
                    await ProcessExistingUsersAsync(users, stoppingToken);
                    users = await GetLatestUsersAsync(_startingDateTime, i, stoppingToken);
                }

                var restrictedUsers = await GetRestrictedUsersAsync(_startingDateTime, 0, stoppingToken);
                for (var i = 1; restrictedUsers.Count > 0; i++)
                {
                    await ProcessRestrictedUsersAsync(users, stoppingToken);
                    restrictedUsers = await GetRestrictedUsersAsync(_startingDateTime, i, stoppingToken);
                }
                
                _startingDateTime = _startingDateTime.Add(_existingUsersLookbackInterval);
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
    /// <param name="date">The <see cref="DateTime"/></param>
    /// <param name="batchNumber">The batch number</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="User"/>s</returns>
    private async Task<List<User>> GetLatestUsersAsync(DateTime date, int batchNumber, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scoreRepository = scope.ServiceProvider.GetRequiredService<IScoreRepository>();
        return await scoreRepository
            .GetUsersFromScoresAfterDate(date, _existingUsersLookbackInterval)
            .Skip(batchNumber * BatchSize)
            .Take(BatchSize)
            .ToListAsync(stoppingToken);
    }

    /// <summary>
    /// Get a batch of restricted <see cref="User"/>s
    /// </summary>
    /// <param name="date">The <see cref="DateTime"/></param>
    /// <param name="batchNumber">The batch number</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="User"/>s</returns>
    private async Task<List<User>> GetRestrictedUsersAsync(DateTime date, int batchNumber,
        CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        return await userRepository
            .GetRestrictedUsersAsync(date, _restrictedUsersLookbackInterval)
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
}