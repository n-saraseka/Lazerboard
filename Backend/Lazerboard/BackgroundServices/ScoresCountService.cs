using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.Redis.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Lazerboard.BackgroundServices;

public class CountService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CountService> _logger;
    private readonly int _updateInterval;

    public CountService(IServiceProvider serviceProvider, ILogger<CountService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        _updateInterval = int.Parse(config["ScoreCountUpdateIntervalMinutes"]!);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var scoreRepository = scope.ServiceProvider.GetRequiredService<IScoreRepository>();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var scoreCacheRepository = scope.ServiceProvider.GetRequiredService<IScoreCacheRepository>();

                var scoresCount = await scoreRepository.GetAll().Select(s => s.Id).CountAsync(stoppingToken);
                _logger.Log(LogLevel.Information, "Scores count: {scoresCount}", scoresCount);
                
                var usersCount = await userRepository.GetAll().Select(u => u.Id).CountAsync(stoppingToken);
                var registeredUsersCount = await userRepository.GetAll()
                    .Where(u => u.UserGroups.Any(g => g == UserGroup.RegisteredUser))
                    .Select(u => u.Id)
                    .CountAsync(stoppingToken);

                var countsData = new CountsData
                {
                    ScoresCount = scoresCount,
                    UsersCount = usersCount,
                    RegisteredUsersCount = registeredUsersCount
                };
                var serializedData = JsonConvert.SerializeObject(countsData);
                await scoreCacheRepository.SetSerializedResponseAsync("counts-data", serializedData, 
                    TimeSpan.FromMinutes(_updateInterval + 30));

                await Task.Delay(TimeSpan.FromMinutes(_updateInterval), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warning, ex, "Scores count service failed!");
            }
        }
    }
}