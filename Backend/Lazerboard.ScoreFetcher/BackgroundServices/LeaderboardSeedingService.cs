using System.Text;
using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Lazerboard.Data.Redis.Repositories.Interfaces;
using Lazerboard.ScoreFetcher.Calculations;
using Lazerboard.ScoreFetcher.Processing;
using osu.Game.Beatmaps;

namespace Lazerboard.ScoreFetcher.BackgroundServices;

public class LeaderboardSeedingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LeaderboardSeedingService> _logger;
    private ISeedingState _seedingState;
    private readonly double _apiInterval;
    private bool _catchUpAfterRestart;
    
    private string? _cursor;
    private int _repeatExponent;

    public LeaderboardSeedingService(IServiceProvider serviceProvider, ILogger<LeaderboardSeedingService> logger, ISeedingState seedingState)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var osuApiConfig = config.GetSection("OsuApi");
        _apiInterval = double.Parse(osuApiConfig["ApiInterval"]);
        _seedingState = seedingState;
        _seedingState.IsSeeding = Environment.GetEnvironmentVariable("EnableDatabaseSeeding") == "true";
        
        var restartConfig = config.GetSection("RestartPolicy");
        _catchUpAfterRestart = bool.Parse(restartConfig["LeaderboardScanCatchUp"]);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var continueSeeding = true;
        if (!_seedingState.IsSeeding) return;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var beatmapsets = await GetBeatmapsetsAsync(stoppingToken);
                
                if (beatmapsets.Count == 0)
                {
                    if (_repeatExponent > 4)
                    {
                        _logger.Log(LogLevel.Information, "No beatmapsets found after {seconds} seconds. Database seeding is complete", 
                            _apiInterval * Math.Pow(2, _repeatExponent));
                        continueSeeding = false;
                    }
                    else
                    {
                        var interval = _apiInterval * Math.Pow(2, _repeatExponent);
                        _logger.Log(LogLevel.Information, "Repeating beatmapset search after {seconds} seconds just to make sure", interval);
                        
                        await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
                        _repeatExponent++;
                    }
                }
                else
                {
                    _repeatExponent = 0;
                    _logger.Log(LogLevel.Information, 
                        "Processing a batch of {beatmapsetCount} beatmapsets ranked between {minDate} and {maxDate}", 
                        beatmapsets.Count,
                        DateOnly.FromDateTime(beatmapsets.Min(bs => bs.RankedDate).Date),
                        DateOnly.FromDateTime(beatmapsets.Max(bs => bs.RankedDate).Date));

                    await SaveBeatmapsetDataAsync(beatmapsets, stoppingToken);
                    
                    foreach (var beatmapset in beatmapsets)
                    {
                        await ProcessBeatmapsetAsync(beatmapset, stoppingToken);
                    }
                }
                
                if (!continueSeeding)
                {
                    _seedingState.IsSeeding = continueSeeding;
                    break;
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
        using var scope = _serviceProvider.CreateScope();
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IOsuApiFetcher>();
        var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
        
        if (_catchUpAfterRestart)
        {
            var beatmapsetId = await dataProcessor.GetSecondHighestBeatmapsetIdAsync(stoppingToken);
            var beatmapset = await apiFetcher.GetBeatmapsetAsync(beatmapsetId, stoppingToken);
            var approvedDate = beatmapset.RankedDate.ToUnixTimeMilliseconds();
            
            _cursor = Convert.ToBase64String(Encoding.Default.GetBytes($"{{\"approved_date\":{approvedDate},\"id\":{beatmapsetId}}}"));
            _catchUpAfterRestart = false;
        }
        
        var beatmapsetsResponse = await apiFetcher.SearchBeatmapsetsAsync(_cursor, stoppingToken);
        _cursor = beatmapsetsResponse.Cursor;
        
        return beatmapsetsResponse.Beatmapsets;
    }

    /// <summary>
    /// Save <see cref="APIBeatmapset"/> data to the database
    /// </summary>
    /// <param name="beatmapsets">List of <see cref="APIBeatmapset"/>s</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task SaveBeatmapsetDataAsync(IReadOnlyCollection<APIBeatmapset> beatmapsets, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var utils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();
        await utils.SaveAllBeatmapsetDataAsync(beatmapsets, stoppingToken);
    }

    /// <summary>
    /// Process beatmapset maps and save the data
    /// </summary>
    /// <param name="beatmapset">The <see cref="APIBeatmapset"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task ProcessBeatmapsetAsync(APIBeatmapset beatmapset, CancellationToken stoppingToken)
    {
        _logger.Log(LogLevel.Information, "Processing beatmapset ID: {beatmapsetID}", beatmapset.Id);

        using (var scope = _serviceProvider.CreateScope())
        {
            var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
            await dataProcessor.ProcessBeatmapsAsync(beatmapset.Beatmaps, stoppingToken);
        }
        
        foreach (var beatmap in beatmapset.Beatmaps)
        {
            var flatWorkingBeatmap = await GetFlatWorkingBeatmapAsync(beatmap.Id, stoppingToken);
            foreach (var val in Enum.GetValues<Mode>())
            {
                if (beatmap.Mode != Mode.Osu && val != beatmap.Mode) continue;
                var scores = await GetBeatmapScoresAsync(beatmap.Id, val, stoppingToken);

                if (scores.Count == 0) continue;
                
                var scoresWithoutPp = scores.Where(s => s.PP == null).ToList();
                var scoresWithPp = scores.Where(s => s.PP != null).ToList();
                        
                foreach (var score in scoresWithoutPp)
                {
                    await CalculateScorePpAsync(score, flatWorkingBeatmap, stoppingToken);
                }
                
                var mergedScores = scoresWithPp.Concat(scoresWithoutPp).ToList();
                await SaveScoreDataAsync(mergedScores, stoppingToken);
            }
        }
    }

    /// <summary>
    /// Get significant <see cref="APIBeatmap"/> leaderboard scores
    /// </summary>
    /// <param name="beatmapId">The <see cref="APIBeatmap"/> ID</param>
    /// <param name="mode">The <see cref="Mode"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task<List<APIScore>> GetBeatmapScoresAsync(int beatmapId, Mode mode, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IOsuApiFetcher>();
        var utils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();
        
        _logger.Log(LogLevel.Information, "Processing beatmap ID: {beatmapID}, mode: {mode}", beatmapId, mode);
        
        var beatmapScores = await apiFetcher.GetBeatmapScoresAsync(beatmapId, mode, 0, stoppingToken);
                        
        var significantScores = await utils.GetSignificantScoresAsync(beatmapScores.Scores, stoppingToken);
        return significantScores.DistinctBy(s => s.Id).ToList();
    }
    
    /// <summary>
    /// Get the <see cref="FlatWorkingBeatmap"/> for <see cref="APIBeatmap"/> ID
    /// </summary>
    /// <param name="beatmapId">The <see cref="APIBeatmap"/> ID</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="FlatWorkingBeatmap"/></returns>
    private async Task<FlatWorkingBeatmap> GetFlatWorkingBeatmapAsync(int beatmapId, CancellationToken stoppingToken)
    {
        _logger.Log(LogLevel.Information, "Getting the FlatWorkingBeatmap for beatmap ID {beatmapId}...", beatmapId);
        using var scope = _serviceProvider.CreateScope();
        var cacheStore = scope.ServiceProvider.GetRequiredService<ICacheStore>();
        var beatmapCacheRepository = scope.ServiceProvider.GetRequiredService<IBeatmapCacheRepository>();
        var osuApiFetcher = scope.ServiceProvider.GetRequiredService<IOsuApiFetcher>();
        
        var filename = await cacheStore.GetBeatmapFileStringAsync(beatmapId, osuApiFetcher, beatmapCacheRepository, stoppingToken);
        return new FlatWorkingBeatmap(filename);
    }

    /// <summary>
    /// Calculate <see cref="APIScore"/>'s PP value and save it to the <see cref="APIScore.PP"/> field
    /// </summary>
    /// <param name="score">The <see cref="APIScore"/></param>
    /// <param name="flatWorkingBeatmap">The <see cref="FlatWorkingBeatmap"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task CalculateScorePpAsync(APIScore score, FlatWorkingBeatmap flatWorkingBeatmap, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scoreProcessor = scope.ServiceProvider.GetRequiredService<IScoreProcessor>();
        
        await scoreProcessor.CalculateScoreAsync(score, flatWorkingBeatmap, stoppingToken);
    }
    
    /// <summary>
    /// Save data from scores to the database
    /// </summary>
    /// <param name="scores">List of <see cref="APIScore"/>s</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task SaveScoreDataAsync(IList<APIScore> scores, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var utils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();
        var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
        
        await utils.SaveUserDataFromScoresAsync(scores,  stoppingToken);
        await dataProcessor.ProcessScoresAsync(scores, ScoreSource.LeaderboardScan, stoppingToken);
    }
}