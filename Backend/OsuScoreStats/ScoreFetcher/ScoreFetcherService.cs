using System.Text;
using OsuScoreStats.Calculations;
using OsuScoreStats.OsuApi.Enums;
using OsuScoreStats.OsuApi.OsuApiEntities;
using OsuScoreStats.Processing;

namespace OsuScoreStats.ScoreFetcher;

public class ScoreFetcherService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScoreFetcherService> _logger;
    private readonly ICacheStore _cacheStore;
    private readonly double _apiInterval;
    private bool _shouldUseFirehose;
    private bool _shouldSeedDatabase;
    private bool _catchUpAfterRestart;
    
    private string? _leaderboardCursor;
    private string? _firehoseCursor;
    private int _repeatExponent;
    private bool _dontCallFirehoseNextTime;

    public ScoreFetcherService(IServiceProvider serviceProvider, ILogger<ScoreFetcherService> logger, ICacheStore cacheStore)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _cacheStore = cacheStore;
        
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var osuApiConfig = config.GetSection("OsuApi");
        _apiInterval = double.Parse(osuApiConfig["ApiInterval"]);
        _shouldUseFirehose = Environment.GetEnvironmentVariable("UseFirehose") == "true";
        _shouldSeedDatabase = Environment.GetEnvironmentVariable("EnableDatabaseSeeding") == "true";
        _catchUpAfterRestart = !_shouldSeedDatabase;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var apiFetcher = scope.ServiceProvider.GetRequiredService<IApiFetcher>();
            var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
            var utils = scope.ServiceProvider.GetRequiredService<ScoreFetchingUtils>();
            
            if (_shouldUseFirehose)
            {
                await FetchFromFirehoseAsync(apiFetcher, dataProcessor, utils, stoppingToken);
            }

            if (_shouldSeedDatabase)
            {
                _shouldUseFirehose = await FetchLeaderboardsAsync(apiFetcher, dataProcessor, utils, stoppingToken);
                if (_shouldUseFirehose)
                {
                    _repeatExponent = 0;
                    _shouldSeedDatabase = false;
                }
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
    /// <returns>True if fetching is complete, false otherwise</returns>
    private async Task<bool> FetchLeaderboardsAsync(IApiFetcher apiFetcher, IDataProcessor dataProcessor, 
        ScoreFetchingUtils utils, CancellationToken stoppingToken)
    {
        _logger.Log(LogLevel.Information, "Searching beatmapsets. Cursor: {cursor}", _leaderboardCursor);
        var beatmapsetsResponse = await apiFetcher.SearchBeatmapsetsAsync(_leaderboardCursor, stoppingToken);
        _leaderboardCursor = beatmapsetsResponse.Cursor;
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
            
            // We call the firehose to catch up with potentially missed scores on the older maps.
            // It should be accurate even if we miss scores on beatmaps that haven't been scanned just yet.
            // We will just catch up on those during the scan. I think?
            if (!_dontCallFirehoseNextTime)
            {
                _logger.Log(LogLevel.Information, "Looking up scores from the firehose. Cursor: {cursor}", _firehoseCursor);
                var firehoseScores = await apiFetcher.GetScoresAsync(_firehoseCursor, stoppingToken);
                if (firehoseScores.Scores.Length == 0)
                {
                    // There are no scores, let the endpoint rest for the next round of scanning the leaderboards.
                    _dontCallFirehoseNextTime = true;
                }
                else
                {
                    var beatmapIds = firehoseScores.Scores.Select(s => s.BeatmapId).Distinct().ToList();
                    var existingBeatmaps = await dataProcessor.GetExistingBeatmapsAsync(beatmapIds, stoppingToken);
                    var existingBeatmapIds = existingBeatmaps.Select(s => s.Id);
                    scores.AddRange(firehoseScores.Scores.Where(s => existingBeatmapIds.Contains(s.BeatmapId)));
                    _firehoseCursor = firehoseScores.Cursor;
                    _dontCallFirehoseNextTime = false;
                }
                await Task.Delay(TimeSpan.FromSeconds(_apiInterval), stoppingToken);
            }
            else
            {
                _dontCallFirehoseNextTime = false;
            }
            
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

     /// <summary>
    /// Get scores from the firehose endpoint
    /// </summary>
    /// <param name="apiFetcher">A <see cref="IApiFetcher"/> service</param>
    /// <param name="dataProcessor">A <see cref="IDataProcessor"/> service</param>
    /// <param name="utils">A <see cref="ScoreFetchingUtils"/> service</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task FetchFromFirehoseAsync(IApiFetcher apiFetcher, IDataProcessor dataProcessor,
        ScoreFetchingUtils utils, CancellationToken stoppingToken)
    {
        _logger.Log(LogLevel.Information, "Looking up scores from the firehose. Cursor: {cursor}", _firehoseCursor);
            
        var scoresResponse = await apiFetcher.GetScoresAsync(_firehoseCursor, stoppingToken);
        var scores = scoresResponse.Scores;

        if (_catchUpAfterRestart)
        {
            if (scores.Length == 0)
            {
                _logger.Log(LogLevel.Warning, "Couldn't get current max score ID");
                _repeatExponent++;
            }
            else
            {
                _repeatExponent = 0;
                // 800k scores is around 6 hours of scores. These would get processed in around 15-20 minutes
                var scoreId = scores.OrderByDescending(s => s.Id).First().Id - 800000;
                _firehoseCursor = Convert.ToBase64String(Encoding.Default.GetBytes($"{{\"id\": {scoreId}}}"));
                _catchUpAfterRestart = false;
            }
        }
        else
        {
            _firehoseCursor = scoresResponse.Cursor;
            _logger.Log(LogLevel.Information, "NewCursor: {cursor}", _firehoseCursor);
            
            await Task.Delay(TimeSpan.FromSeconds(_apiInterval), stoppingToken);
            
            var significantScores = await utils.GetSignificantScoresAsync(scores, stoppingToken);
            _logger.Log(LogLevel.Information, "SignificantScoreCount: {count}", significantScores.Count);

            if (significantScores.Count > 0)
            {
                _repeatExponent = 0;
            
                await utils.SaveUserDataFromScoresAsync(significantScores,  stoppingToken);
            
                var beatmapIds = significantScores.Select(s => s.BeatmapId).Distinct().ToList();
                var existingBeatmaps = await dataProcessor.GetExistingBeatmapsAsync(beatmapIds, stoppingToken);
                var newBeatmapIds = beatmapIds.Where(id => !existingBeatmaps.Select(b => b.Id).Contains(id)).ToList();
                _logger.Log(LogLevel.Information, "NewBeatmapIds: {ids}", newBeatmapIds);
                var beatmaps = await apiFetcher.GetBeatmapsAsync(newBeatmapIds, stoppingToken);
            
                var beatmapsets = beatmaps.Select(b => b.Beatmapset).Distinct().ToList();
                await utils.SaveAllBeatmapsetDataAsync(beatmapsets, stoppingToken);
                await dataProcessor.ProcessBeatmapsAsync(beatmaps, stoppingToken);
            
                await dataProcessor.ProcessScoresAsync(significantScores, stoppingToken);
            }
            else
            {
                _logger.Log(LogLevel.Information, "No significant scores found after {interval} seconds. Repeating in {nextInterval} seconds", 
                    _apiInterval * Math.Pow(2, _repeatExponent), _apiInterval * Math.Pow(2, _repeatExponent + 1));
                _repeatExponent++;
            }
        }
        
        var interval = _apiInterval * Math.Pow(2, _repeatExponent);
        
        await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
    }
}