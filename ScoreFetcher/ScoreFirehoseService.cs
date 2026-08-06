using OsuScoreStats.Calculations;

namespace OsuScoreStats.ScoreFetcher;

public class ScoreFirehoseService : BackgroundService
{
    private IServiceProvider _serviceProvider;
    private ILogger<ScoreLeaderboardService> _logger;
    private string? _cursor;
    private double _apiInterval;
    private const int RepeatAfterSeconds = 10;
    
    public ScoreFirehoseService(IServiceProvider serviceProvider, ILogger<ScoreLeaderboardService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        using var scope = _serviceProvider.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
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
            
            _logger.Log(LogLevel.Information, "Looking up scores. Cursor: {cursor}", _cursor);
            
            var scoresResponse = await apiFetcher.GetScoresAsync(_cursor, stoppingToken);
            _cursor = scoresResponse.Cursor;
            var scores = scoresResponse.Scores;
            _logger.Log(LogLevel.Information, "NewCursor: {cursor}", _cursor);
            
            await Task.Delay(TimeSpan.FromSeconds(_apiInterval), stoppingToken);
            
            var significantScores = await utils.GetSignificantScoresAsync(scores, stoppingToken);
            _logger.Log(LogLevel.Information, "SignificantScoreCount: {count}", significantScores.Count);
            await utils.SaveUserDataFromScoresAsync(significantScores,  stoppingToken);
            
            var beatmapIds = significantScores.Select(s => s.BeatmapId).Distinct();
            var existingBeatmaps = await dataProcessor.GetExistingBeatmapsAsync(beatmapIds, stoppingToken);
            var newBeatmapIds = beatmapIds.Where(id => !existingBeatmaps.Select(b => b.Id).Contains(id)).ToList();
            _logger.Log(LogLevel.Information, "NewBeatmapIds: {ids}", newBeatmapIds);
            var beatmaps = await apiFetcher.GetBeatmapsAsync(newBeatmapIds, stoppingToken);
            
            var beatmapsets = beatmaps.Select(b => b.Beatmapset).Distinct().ToList();
            await utils.SaveAllBeatmapsetDataAsync(beatmapsets, stoppingToken);
            await dataProcessor.ProcessBeatmapsAsync(beatmaps, stoppingToken);
            
            await dataProcessor.ProcessScoresAsync(significantScores, stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(RepeatAfterSeconds), stoppingToken);
        }
    }
}