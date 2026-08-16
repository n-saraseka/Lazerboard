using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OsuScoreStats.ScoreFetcher.Processing;

namespace OsuScoreStats.ScoreFetcher.ScoreFetcher;

public class FirehoseService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FirehoseService> _logger;
    private ISeedingState _seedingState;
    private readonly double _apiInterval;
    private bool _catchUpAfterRestart;

    private string? _cursor;
    private int _repeatExponent;
    
    public FirehoseService(IServiceProvider serviceProvider, ILogger<FirehoseService> logger, ISeedingState seedingState)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _seedingState = seedingState;
        
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var osuApiConfig = config.GetSection("OsuApi");
        _apiInterval = double.Parse(osuApiConfig["ApiInterval"]);
        _catchUpAfterRestart = Environment.GetEnvironmentVariable("EnableDatabaseSeeding") == "false";
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var apiFetcher = scope.ServiceProvider.GetRequiredService<IApiFetcher>();
            var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
            var utils = scope.ServiceProvider.GetRequiredService<ScoreFetchingUtils>();

            await FetchFromFirehoseAsync(apiFetcher, dataProcessor, utils, stoppingToken);
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
        _logger.Log(LogLevel.Information, "Looking up scores from the firehose. Cursor: {cursor}", _cursor);
            
        var scoresResponse = await apiFetcher.GetScoresAsync(_cursor, stoppingToken);
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
                _cursor = Convert.ToBase64String(Encoding.Default.GetBytes($"{{\"id\": {scoreId}}}"));
                _catchUpAfterRestart = false;
            }
        }
        else
        {
            _cursor = scoresResponse.Cursor;
            _logger.Log(LogLevel.Information, "NewCursor: {cursor}", _cursor);
            
            if (scores.Length == 0)
            {
                _logger.Log(LogLevel.Information, "No scores found after {interval} seconds. Repeating in {nextInterval} seconds", 
                    _apiInterval * Math.Pow(2, _repeatExponent), _apiInterval * Math.Pow(2, _repeatExponent + 1));
                _repeatExponent++;
                var interval = _apiInterval * Math.Pow(2, _repeatExponent);
                await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
                return;
            }
            _repeatExponent = 0;

            // If we are in the middle of seeding the database, we only care for scores set on existing beatmaps
            // to catch up on new scores from already processed maps.
            if (_seedingState.IsSeeding)
            {
                var beatmapIds = scores.Select(s => s.BeatmapId).Distinct().ToList();
                var existingBeatmaps = await dataProcessor.GetExistingBeatmapsAsync(beatmapIds, stoppingToken);
                var existingBeatmapIds = existingBeatmaps.Select(s => s.Id).ToList();
                scores = scores.Where(s => existingBeatmapIds.Contains(s.BeatmapId)).ToArray();
            }

            var significantScores = await utils.GetSignificantScoresAsync(scores, stoppingToken);
            _logger.Log(LogLevel.Information, "SignificantScoreCount: {count}", significantScores.Count);

            if (significantScores.Count == 0) return;
            
            await utils.SaveUserDataFromScoresAsync(significantScores,  stoppingToken);
                
            // We only need to catch up on new beatmaps in case we've finished seeding the database
            if (!_seedingState.IsSeeding)
            {
                var beatmapIds = significantScores.Select(s => s.BeatmapId).Distinct().ToList();
                var existingBeatmaps = await dataProcessor.GetExistingBeatmapsAsync(beatmapIds, stoppingToken);
                var newBeatmapIds = beatmapIds.Where(id => !existingBeatmaps.Select(b => b.Id).Contains(id)).ToList();
                _logger.Log(LogLevel.Information, "NewBeatmapIds: {ids}", newBeatmapIds);
                var beatmaps = await apiFetcher.GetBeatmapsAsync(newBeatmapIds, stoppingToken);
            
                var beatmapsets = beatmaps.Select(b => b.Beatmapset).Distinct().ToList();
                await utils.SaveAllBeatmapsetDataAsync(beatmapsets, stoppingToken);
                await dataProcessor.ProcessBeatmapsAsync(beatmaps, stoppingToken);
            }
            
            await dataProcessor.ProcessScoresAsync(significantScores, stoppingToken);
        }
    }
}