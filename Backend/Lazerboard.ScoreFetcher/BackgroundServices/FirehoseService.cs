using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Lazerboard.ScoreFetcher.Processing;

namespace Lazerboard.ScoreFetcher.BackgroundServices;

public class FirehoseService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FirehoseService> _logger;
    private ISeedingState _seedingState;
    private readonly double _apiInterval;
    private bool _catchUpAfterRestart = true;
    private bool _catchUpOnExistingScores;

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
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var apiFetcher = scope.ServiceProvider.GetRequiredService<IApiFetcher>();
            var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
            var utils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();

            try
            {
                if (_seedingState.IsSeeding)
                {
                    await FetchExistingBeatmapScoresAsync(apiFetcher, dataProcessor, utils, stoppingToken);
                    if (!_catchUpOnExistingScores)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                    }
                }
                else
                {
                    await FetchFromFirehoseAsync(apiFetcher, dataProcessor, utils, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex, "Firehose service failed!");
                throw;
            }
        }
    }
    
    /// <summary>
    /// Get scores from the firehose endpoint
    /// </summary>
    /// <param name="apiFetcher">A <see cref="IApiFetcher"/> service</param>
    /// <param name="dataProcessor">A <see cref="IDataProcessor"/> service</param>
    /// <param name="utils">A <see cref="IScoreFetchingUtils"/> service</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task FetchFromFirehoseAsync(IApiFetcher apiFetcher, IDataProcessor dataProcessor,
        IScoreFetchingUtils utils, CancellationToken stoppingToken)
    {
        if (_catchUpAfterRestart)
        {
            await GetRestartCursorAsync(dataProcessor, stoppingToken);
        }
            
        var scoresResponse = await apiFetcher.GetScoresAsync(_cursor, stoppingToken);
        var scores = scoresResponse.Scores;

        var minDate = scores.Min(s => s.Date);
        var maxDate = scores.Max(s => s.Date);
        _logger.Log(LogLevel.Information, "Processing a batch of {scoresCount} scores between {minScoreDate} and {maxScoreDate}", 
            scores.Length, minDate, maxDate);

        var significantScores = await utils.GetSignificantScoresAsync(scores, stoppingToken);

        if (significantScores.Count == 0)
        {
            _logger.Log(LogLevel.Information, "No significant scores found after {interval} seconds. Repeating in {nextInterval} seconds", 
                _apiInterval * Math.Pow(2, _repeatExponent), _apiInterval * Math.Pow(2, _repeatExponent + 1));
            _repeatExponent++;
            var interval = _apiInterval * Math.Pow(2, _repeatExponent);
            await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
            return;
        }
        _repeatExponent = 0;
            
        await utils.SaveUserDataFromScoresAsync(significantScores,  stoppingToken);
            
        // Process new beatmaps and beatmapsets first if necessary
        var beatmapIds = significantScores.Select(s => s.BeatmapId).Distinct().ToList();
        var existingBeatmaps = await dataProcessor.GetExistingBeatmapsAsync(beatmapIds, stoppingToken);
        var newBeatmapIds = beatmapIds.Where(id => !existingBeatmaps.Select(b => b.Id).Contains(id)).ToList();

        if (newBeatmapIds.Count > 0)
        {
            var beatmaps = await apiFetcher.GetBeatmapsAsync(newBeatmapIds, stoppingToken);
            var beatmapsets = beatmaps.Select(b => b.Beatmapset).Distinct().ToList();
            await utils.SaveAllBeatmapsetDataAsync(beatmapsets, stoppingToken);
            await dataProcessor.ProcessBeatmapsAsync(beatmaps, stoppingToken);
        }
            
        await dataProcessor.ProcessScoresAsync(significantScores, stoppingToken);
    }

    /// <summary>
    /// Get scores from the firehose endpoint and filter them to ones from maps that are already in the database
    /// </summary>
    /// <param name="apiFetcher">A <see cref="IApiFetcher"/> service</param>
    /// <param name="dataProcessor">A <see cref="IDataProcessor"/> service</param>
    /// <param name="utils">A <see cref="IScoreFetchingUtils"/> service</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task FetchExistingBeatmapScoresAsync(IApiFetcher apiFetcher, IDataProcessor dataProcessor,
        IScoreFetchingUtils utils, CancellationToken stoppingToken)
    {
        if (_catchUpAfterRestart)
        {
            await GetRestartCursorAsync(dataProcessor, stoppingToken);
        }

        _catchUpOnExistingScores = true;
        
        var scoresResponse = await apiFetcher.GetScoresAsync(_cursor, stoppingToken);
        var scores = scoresResponse.Scores;
        _cursor = scoresResponse.Cursor;
        
        var beatmapIds = scores.Select(s => s.BeatmapId).Distinct().ToList();
        var existingBeatmapIds = await dataProcessor.GetBeatmapIdsWithScoresAsync(beatmapIds, stoppingToken);

        var scoresToProcess = scores.Where(s => existingBeatmapIds.Contains(s.BeatmapId)).ToList();
        if (scoresToProcess.Count > 0)
        {
            var minDate = scoresToProcess.Min(s => s.Date);
            var maxDate = scoresToProcess.Max(s => s.Date);
            _logger.Log(LogLevel.Information, "Processing a batch of {scoresCount} scores between {minScoreDate} and {maxScoreDate}", 
                scoresToProcess.Count, minDate, maxDate);
            
            var significantScores = await utils.GetSignificantScoresAsync(scoresToProcess, stoppingToken);

            if (significantScores.Count == 0)
            {
                _catchUpOnExistingScores = false;
                return;
            }
            
            await utils.SaveUserDataFromScoresAsync(significantScores,  stoppingToken);
            await dataProcessor.ProcessScoresAsync(significantScores, stoppingToken);
        }
    }

    private async Task GetRestartCursorAsync(IDataProcessor dataProcessor, CancellationToken stoppingToken)
    {
        var maxId = await dataProcessor.GetMaxScoreIdAsync(stoppingToken);
        _cursor = Convert.ToBase64String(Encoding.Default.GetBytes($"{{\"id\": {maxId}}}"));
        _catchUpAfterRestart = false;
    }
}