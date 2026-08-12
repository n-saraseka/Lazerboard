using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OsuScoreStats.Shared.Calculations;
using OsuScoreStats.Shared.OsuApi.Enums;
using OsuScoreStats.Shared.OsuApi.OsuApiEntities;
using OsuScoreStats.Shared.Processing;

namespace OsuScoreStats.DatabaseSeeder;

public class LeaderboardFetcherService : BackgroundService
{
    private IServiceProvider _serviceProvider;
    private ILogger<LeaderboardFetcherService> _logger;
    private IHostApplicationLifetime _applicationLifetime;
    private string? _cursor;
    private double _apiInterval;
    private int _repeatExponent;
    
    public LeaderboardFetcherService(IServiceProvider serviceProvider, ILogger<LeaderboardFetcherService> logger, IConfiguration config, 
        IHostApplicationLifetime applicationLifetime)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _applicationLifetime = applicationLifetime;
        
        _apiInterval = double.Parse(config["OsuApiInterval"]);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var apiFetcher = scope.ServiceProvider.GetRequiredService<IApiFetcher>();
            var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
            var cacheStore = scope.ServiceProvider.GetRequiredService<ICacheStore>();
            var utils = scope.ServiceProvider.GetRequiredService<ScoreFetchingUtils>();
            
            cacheStore.CheckCache();
            var shouldStop = await FetchLeaderboardsAsync(apiFetcher, dataProcessor, utils, stoppingToken);
            if (shouldStop) _applicationLifetime.StopApplication();
        }
    }
    
    /// <summary>
    /// Scan scores from all existing beatmap leaderboards
    /// </summary>
    /// <param name="apiFetcher">A <see cref="IApiFetcher"/> service</param>
    /// <param name="dataProcessor">A <see cref="IDataProcessor"/> service</param>
    /// <param name="utils">A <see cref="ScoreFetchingUtils"/> service</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>True if fetching is complete, false otherwise</returns>
    private async Task<bool> FetchLeaderboardsAsync(IApiFetcher apiFetcher, IDataProcessor dataProcessor, 
        ScoreFetchingUtils utils, CancellationToken stoppingToken)
    {
        _logger.Log(LogLevel.Information, "Searching beatmapsets. Cursor: {cursor}", _cursor);
        var beatmapsetsResponse = await apiFetcher.SearchBeatmapsetsAsync(_cursor, stoppingToken);
        _cursor = beatmapsetsResponse.Cursor;
        await Task.Delay(TimeSpan.FromSeconds(_apiInterval), stoppingToken);
        
        var beatmapsets = beatmapsetsResponse.Beatmapsets;

        if (beatmapsets.Count == 0)
        {
            _logger.Log(LogLevel.Information, "No beatmapsets found.");
            if (_repeatExponent >= 4)
            {
                _logger.Log(LogLevel.Information, "No beatmapsets found after {seconds} seconds. Database seeding is complete", 
                    _apiInterval * Math.Pow(2, _repeatExponent));
                return true;
            }
            
            var interval = _apiInterval * Math.Pow(2, _repeatExponent);
            _logger.Log(LogLevel.Information, "Repeating beatmapset search after {seconds} seconds just to make sure", interval);
            
            await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
            _repeatExponent++;
        }
        else
        {
            await utils.SaveAllBeatmapsetDataAsync(beatmapsets, stoppingToken);
        
            var beatmaps = new List<APIBeatmap>();
            foreach (var beatmapset in beatmapsets)
                beatmaps.AddRange(beatmapset.Beatmaps);
            await dataProcessor.ProcessBeatmapsAsync(beatmaps, stoppingToken);

            var scores = new List<APIScore>();
            foreach (var beatmap in beatmaps)
            {
                var beatmapScores = new List<BeatmapScores>();
            
                foreach (var val in Enum.GetValues<Mode>())
                {
                    if (beatmap.Mode != Mode.Osu && beatmap.Mode != val) continue;
                    _logger.Log(LogLevel.Information, "Getting leaderboard scores. BeatmapID: {id}; Mode: {mode}", beatmap.Id, val);
                    beatmapScores.Add(await apiFetcher.GetBeatmapScoresAsync(beatmap, val, 0, stoppingToken));
                    await Task.Delay(TimeSpan.FromSeconds(_apiInterval), stoppingToken);
                }
            
                scores.AddRange(beatmapScores.SelectMany(bs => bs.Scores));
                await Task.Delay(TimeSpan.FromSeconds(_apiInterval), stoppingToken);
            }
        
            var significantScores = await utils.GetSignificantScoresAsync(scores, stoppingToken);
            _logger.Log(LogLevel.Information, "SignificantScoreCount: {count}", significantScores.Count);

            if (significantScores.Count > 0)
            {
                await utils.SaveUserDataFromScoresAsync(significantScores,  stoppingToken);
                await dataProcessor.ProcessScoresAsync(significantScores, stoppingToken);
            }
        }

        return false;
    }
}