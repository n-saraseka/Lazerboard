using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.Redis.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lazerboard.ScoreFetcher.BackgroundServices;

public class ScoresCountService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FirehoseService> _logger;
    private readonly int _updateInterval;

    public ScoresCountService(IServiceProvider serviceProvider, ILogger<FirehoseService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        _updateInterval = int.Parse(config["ScoreCountUpdateIntervalMinutes"]);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var scoresRepository = scope.ServiceProvider.GetRequiredService<IScoreRepository>();
                var scoreCacheRepository = scope.ServiceProvider.GetRequiredService<IScoreCacheRepository>();

                var scoresCount = await scoresRepository.GetAll().CountAsync(stoppingToken);
                _logger.Log(LogLevel.Information, "Scores count: {scoresCount}", scoresCount);
                await scoreCacheRepository.SetScoresCountAsync(scoresCount, TimeSpan.FromMinutes(_updateInterval + 10));

                await Task.Delay(TimeSpan.FromMinutes(_updateInterval), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warning, ex, "Scores count service failed!");
            }
        }
    }
}