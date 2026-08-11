using System.Text;
using OsuScoreStats.Calculations;
using OsuScoreStats.OsuApi.Enums;
using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.ScoreFetcher;

public class ScoreFetcherService : BackgroundService
{
    private IServiceProvider _serviceProvider;
    private ILogger<ScoreFetcherService> _logger;
    private string? _cursor;
    private double _apiInterval;
    private int _repeatExponent;
    private bool _shouldUseFirehose;
    private bool _postRestart;
    private string _firehosePath;

    public ScoreFetcherService(IServiceProvider serviceProvider, ILogger<ScoreFetcherService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        _apiInterval = double.Parse(config["OsuApiInterval"]);
        
        _firehosePath = config["FirehosePath"];
        _shouldUseFirehose = File.Exists(_firehosePath);

        if (_shouldUseFirehose)
        {
            _logger.Log(LogLevel.Information, "Firehose file found. Fetching scores from the firehose");
            _postRestart = true;
        }
        else
            _logger.Log(LogLevel.Information, "Firehose file not found. Scanning all existing leaderboards");
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
            if (_shouldUseFirehose)
                await FetchFromFirehoseAsync(apiFetcher, dataProcessor, utils, stoppingToken);
            else
                await FetchLeaderboardsAsync(apiFetcher, dataProcessor, utils, stoppingToken);
        }
    }

    /// <summary>
    /// Scan scores from all existing beatmap leaderboards
    /// </summary>
    /// <param name="apiFetcher">A <see cref="IApiFetcher"/> service</param>
    /// <param name="dataProcessor">A <see cref="IDataProcessor"/> service</param>
    /// <param name="utils">A <see cref="ScoreFetchingUtils"/> service</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task FetchLeaderboardsAsync(IApiFetcher apiFetcher, IDataProcessor dataProcessor, 
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
                _logger.Log(LogLevel.Information, "No beatmapsets found after {seconds} seconds. Switching to using the firehose", 
                    _apiInterval * Math.Pow(2, _repeatExponent));
                File.CreateText(_firehosePath).Close();
                _cursor = null;
                _repeatExponent = 0;
                _shouldUseFirehose = true;
                return;
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
            await utils.SaveUserDataFromScoresAsync(significantScores,  stoppingToken);
            await dataProcessor.ProcessScoresAsync(significantScores, stoppingToken);
        }
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
        _logger.Log(LogLevel.Information, "Looking up scores. Cursor: {cursor}", _cursor);
            
        var scoresResponse = await apiFetcher.GetScoresAsync(_cursor, stoppingToken);
        var scores = scoresResponse.Scores;

        if (_postRestart)
        {
            if (scores.Length == 0)
            {
                _logger.Log(LogLevel.Warning, "Couldn't get current max score ID");
            }
            else
            {
                // 4.8 million scores is around a day of scores. These would get processed in around 1 hour and 20 minutes
                var scoreId = scores.OrderByDescending(s => s.Id).First().Id - 4.8e6;
                _cursor = Convert.ToBase64String(Encoding.Default.GetBytes($"{{\"id\": {scoreId}}}"));
            }
        }
        else
        {
            _cursor = scoresResponse.Cursor;
            _logger.Log(LogLevel.Information, "NewCursor: {cursor}", _cursor);
            
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