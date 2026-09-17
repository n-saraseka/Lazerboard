using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
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
    private readonly TimeSpan _lookbackInterval;
    private DateTime _startingDateTime;
    
    public UserUpdatesService(IServiceProvider serviceProvider, ILogger<UserUpdatesService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var updateHours = config.GetValue<double>("UserUpdatesIntervalHours");
        _lookbackInterval = TimeSpan.FromHours(updateHours);
        _startingDateTime = DateTime.UtcNow - _lookbackInterval;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var users = await GetUsersAsync(_startingDateTime, 0, stoppingToken);
                for (var i = 1; users.Count > 0; i++)
                {
                    await ProcessUsersAsync(users, stoppingToken);
                    users = await GetUsersAsync(_startingDateTime, i, stoppingToken);
                }
                _startingDateTime = _startingDateTime.Add(_lookbackInterval);
                await Task.Delay(_lookbackInterval, stoppingToken);
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
    /// Get a batch of <see cref="User"/>s from scores after specified date
    /// </summary>
    /// <param name="date">The <see cref="DateTime"/></param>
    /// <param name="batchNumber">The batch number</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="User"/>s</returns>
    private async Task<List<User>> GetUsersAsync(DateTime date, int batchNumber, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scoreRepository = scope.ServiceProvider.GetRequiredService<IScoreRepository>();
        return await scoreRepository
            .GetUsersFromScoresAfterDate(date, _lookbackInterval)
            .Skip(batchNumber * BatchSize)
            .Take(BatchSize)
            .ToListAsync(stoppingToken);
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
        await userUtils.ProcessUsersAsync(users, false, stoppingToken);
    }
}