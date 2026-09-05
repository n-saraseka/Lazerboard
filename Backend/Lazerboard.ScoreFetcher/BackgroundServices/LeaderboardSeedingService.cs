using System.Text;
using Lazerboard.Data.Database.Entities.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Lazerboard.ScoreFetcher.Processing;

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
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IApiFetcher>();
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
        foreach (var beatmap in beatmapset.Beatmaps)
        {
            foreach (var val in Enum.GetValues<Mode>())
            {
                if (beatmap.Mode != Mode.Osu && val != beatmap.Mode) continue;
                await ProcessBeatmapScoresAsync(beatmap.Id, val, stoppingToken);
            }
        }
    }

    /// <summary>
    /// Process <see cref="APIBeatmap"/> leaderboard scores and save significant ones to the database
    /// </summary>
    /// <param name="beatmapId">The <see cref="APIBeatmap"/> ID</param>
    /// <param name="mode">The <see cref="Mode"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task ProcessBeatmapScoresAsync(int beatmapId, Mode mode, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IApiFetcher>();
        var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
        var utils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();
        
        _logger.Log(LogLevel.Information, "Processing beatmap ID: {beatmapID}, mode: {mode}", beatmapId, mode);
        
        var beatmapScores = await apiFetcher.GetBeatmapScoresAsync(beatmapId, mode, 0, stoppingToken);
                        
        var significantScores = await utils.GetSignificantScoresAsync(beatmapScores.Scores, stoppingToken);
        significantScores = significantScores.DistinctBy(s => s.Id).ToList();

        if (significantScores.Count > 0)
        {
            await utils.SaveUserDataFromScoresAsync(significantScores,  stoppingToken);
            await dataProcessor.ProcessScoresAsync(significantScores, ScoreSource.LeaderboardScan, stoppingToken);
        }
    }
}