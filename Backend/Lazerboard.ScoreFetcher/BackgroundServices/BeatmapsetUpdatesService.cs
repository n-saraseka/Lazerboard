using System.Text;
using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Lazerboard.ScoreFetcher.Processing;

namespace Lazerboard.ScoreFetcher.BackgroundServices;

public class BeatmapsetUpdatesService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BeatmapsetUpdatesService> _logger;
    private readonly double _apiInterval;
    private bool _catchUpAfterRestart = true;
    
    private string? _cursor;
    private int _repeatExponent;

    public BeatmapsetUpdatesService(IServiceProvider serviceProvider, ILogger<BeatmapsetUpdatesService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var externalApisConfig = config.GetSection("ExternalApis");
        var osuApiConfig = externalApisConfig.GetSection("OsuApi");
        _apiInterval = osuApiConfig.GetValue<double>("ApiInterval");
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var shouldUpdate = await ShouldUpdateBeatmapsetsAsync(stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!shouldUpdate)
            {
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                shouldUpdate = await ShouldUpdateBeatmapsetsAsync(stoppingToken);
                continue;
            }
            try
            {
                var beatmapsets = await GetBeatmapsetsAsync(stoppingToken);
                
                if (beatmapsets.Count == 0)
                {
                    var interval = _apiInterval * Math.Pow(2, _repeatExponent);
                    _logger.Log(LogLevel.Information, "No new beatmapsets found. Repeating after {seconds}", interval);
                        
                    await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
                    // Exponential backoff exponent is capped to 10 (~17 minute intervals)
                    _repeatExponent = Math.Min(_repeatExponent + 1, 10);
                }
                else
                {
                    _repeatExponent = 0;
                    _logger.Log(LogLevel.Information, 
                        "Processing a batch of {beatmapsetCount} beatmapsets ranked between {minDate} and {maxDate}", 
                        beatmapsets.Count,
                        DateOnly.FromDateTime(beatmapsets.Min(bs => bs.RankedDate).Date),
                        DateOnly.FromDateTime(beatmapsets.Max(bs => bs.RankedDate).Date));

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var scoreFetchingUtils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();
                        await scoreFetchingUtils.SaveAllBeatmapsetDataAsync(beatmapsets, ScanEventType.MainSeedingStarted, stoppingToken);
                    }
                    
                    foreach (var beatmapset in beatmapsets)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var beatmapUtils = scope.ServiceProvider.GetRequiredService<IBeatmapUtils>();
                        await beatmapUtils.ProcessBeatmapsetAsync(beatmapset, ScanEventType.MainSeedingStarted, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "Leaderboard seeding service failed!");
                throw;
            }
        }
    }
    
    /// <summary>
    /// Get <see cref="APIBeatmapset"/>s from the search API
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="APIBeatmapset"/>s</returns>
    private async Task<List<APIBeatmapset>> GetBeatmapsetsAsync(CancellationToken stoppingToken)
    {
        var startingBeatmapset = await GetStartingBeatmapsetAsync(stoppingToken);
        using var scope = _serviceProvider.CreateScope();
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IOsuApiFetcher>();
        
        if (startingBeatmapset is null)
        {
            _cursor = null;
        }
        else if (_catchUpAfterRestart)
        {
            if (startingBeatmapset.RankedDate is null)
            {
                var apiBeatmapset = await apiFetcher.GetBeatmapsetAsync(startingBeatmapset.Id, stoppingToken);
                startingBeatmapset.RankedDate = apiBeatmapset.RankedDate;
            }
            
            var approvedDate = startingBeatmapset.RankedDate.Value.ToUnixTimeMilliseconds();
            _cursor = Convert.ToBase64String(Encoding.Default.GetBytes($"{{\"approved_date\":{approvedDate},\"id\":{startingBeatmapset.Id}}}"));
            _catchUpAfterRestart = false;
        }
        
        var beatmapsetsResponse = await apiFetcher.SearchBeatmapsetsAsync(_cursor, stoppingToken);
        _cursor = beatmapsetsResponse.Cursor;
        
        return beatmapsetsResponse.Beatmapsets;
    }

    private async Task<Beatmapset?> GetStartingBeatmapsetAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var beatmapsetRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetRepository>();
        var startingBeatmapset = await beatmapsetRepository.GetLatestMainProcessedMapsetAsync(stoppingToken);

        if (startingBeatmapset is null)
        {
            var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
            var beatmapsetId = await dataProcessor.GetSecondHighestBeatmapsetIdAsync(stoppingToken);
            startingBeatmapset = await beatmapsetRepository.GetByIdAsync(beatmapsetId, stoppingToken);
        }
        
        return startingBeatmapset;
    }

    /// <summary>
    /// Check whether the updater service should proceed or not
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>True if there are no beatmapsets with null <see cref="Beatmapset.RankedDate"/>s and a scan hasn't been finished</returns>
    private async Task<bool> ShouldUpdateBeatmapsetsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var beatmapsetRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetRepository>();
        var beatmapsetLogRepository = scope.ServiceProvider.GetRequiredService<IBeatmapsetScanLogRepository>();
        
        var nullBeatmapset = await beatmapsetRepository.GetLatestBeatmapsetWithNullRankAsync(stoppingToken);
        var finishedScan = await beatmapsetLogRepository.GetLatestFinishedScanAsync(stoppingToken);

        return !(nullBeatmapset is null && finishedScan is null);
    }
}