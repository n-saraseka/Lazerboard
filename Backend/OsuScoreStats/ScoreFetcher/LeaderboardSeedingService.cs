using OsuScoreStats.OsuApi.Enums;
using OsuScoreStats.OsuApi.OsuApiEntities;
using OsuScoreStats.Processing;

namespace OsuScoreStats.ScoreFetcher;

public class LeaderboardSeedingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LeaderboardSeedingService> _logger;
    private ISeedingState _seedingState;
    private readonly double _apiInterval;
    
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
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_seedingState.IsSeeding) return;
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var apiFetcher = scope.ServiceProvider.GetRequiredService<IApiFetcher>();
            var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
            var utils = scope.ServiceProvider.GetRequiredService<ScoreFetchingUtils>();

            var continueSeeding = await FetchLeaderboardsAsync(apiFetcher, dataProcessor, utils, stoppingToken);
            if (!continueSeeding)
            {
                _seedingState.IsSeeding = continueSeeding;
                break;
            }
        }
    }

    /// <summary>
    /// Scan scores from all existing beatmap leaderboards
    /// </summary>
    /// <param name="apiFetcher">A <see cref="IApiFetcher"/> service</param>
    /// <param name="dataProcessor">A <see cref="IDataProcessor"/> service</param>
    /// <param name="utils">A <see cref="ScoreFetchingUtils"/> service</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>False if there is no data after multiple retries, true otherwise</returns>
    private async Task<bool> FetchLeaderboardsAsync(IApiFetcher apiFetcher, IDataProcessor dataProcessor, 
        ScoreFetchingUtils utils, CancellationToken stoppingToken)
    {
        _logger.Log(LogLevel.Information, "Searching beatmapsets. Cursor: {cursor}", _cursor);
        var beatmapsetsResponse = await apiFetcher.SearchBeatmapsetsAsync(_cursor, stoppingToken);
        _cursor = beatmapsetsResponse.Cursor;
        
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
                }
            
                scores.AddRange(beatmapScores.SelectMany(bs => bs.Scores));
            }
        
            var significantScores = await utils.GetSignificantScoresAsync(scores, stoppingToken);
            significantScores = significantScores.DistinctBy(s => s.Id).ToList();
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