using System.Text;
using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Lazerboard.ScoreFetcher.Processing;
using Microsoft.Extensions.Configuration;

namespace Lazerboard.ScoreFetcher.BackgroundServices;

public class FirehoseService: BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BeatmapsetUpdatesService> _logger;
    private readonly double _apiInterval;
    private bool _catchUpAfterRestart = true;
    private bool _catchUpOnExistingBeatmapScores;

    private string? _cursor;
    private int _repeatExponent;
    
    public FirehoseService(IServiceProvider serviceProvider, ILogger<BeatmapsetUpdatesService> logger)
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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var scores = await FetchExistingBeatmapScoresAsync(stoppingToken);
                if (scores.Count > 0)
                {
                    var significantScores = await GetExistingBeatmapScoresAsync(scores, stoppingToken);
                    if (significantScores.Count == 0)
                    {
                        if (_catchUpOnExistingBeatmapScores)
                        {
                            continue;
                        }
                        
                        var interval = _apiInterval * Math.Pow(2, _repeatExponent);
                        _logger.Log(LogLevel.Information, "No significant scores found. Repeating after {seconds} found", interval);
                        
                        await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
                        // Exponential backoff exponent is capped to 8 (~4 minute intervals)
                        _repeatExponent = Math.Min(_repeatExponent + 1, 8);
                        continue;
                    }
                    
                    var scoresWithoutPp = significantScores.Where(s => s.PP == null).ToList();
                    var scoresWithPp = significantScores.Where(s => s.PP != null).ToList();

                    var groupedByBeatmapId = scoresWithoutPp.GroupBy(s => s.BeatmapId).ToList();
                    foreach (var group in groupedByBeatmapId)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var utils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();
                        var scoreProcessor = scope.ServiceProvider.GetRequiredService<IScoreProcessor>();
                        
                        var flatWorkingBeatmap = await utils.GetFlatWorkingBeatmapAsync(group.Key, stoppingToken);
                        var groupScores = group.ToList();
                        foreach (var score in groupScores)
                        {
                            await scoreProcessor.CalculateScoreAsync(score, flatWorkingBeatmap, stoppingToken);
                        }
                    }
                        
                    var mergedScores = scoresWithPp.Concat(scoresWithoutPp).ToList();
                    await SaveExistingBeatmapScoresAsync(mergedScores, stoppingToken);
                }
                if (!_catchUpOnExistingBeatmapScores)
                {
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
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
    /// Get scores from the firehose endpoint and filter them to ones from maps that are already in the database
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task<List<APIScore>> FetchExistingBeatmapScoresAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IOsuApiFetcher>();
        
        if (_catchUpAfterRestart)
        {
            await GetRestartCursorAsync(dataProcessor, stoppingToken);
            _catchUpOnExistingBeatmapScores = true;
        }
        
        var scoresResponse = await apiFetcher.GetScoresAsync(_cursor, stoppingToken);
        var scores = scoresResponse.Scores;
        
        if (_cursor is null && _catchUpAfterRestart)
        {
            if (scores.Length == 0)
            {
                _logger.Log(LogLevel.Warning, "Couldn't get max score ID from firehose!");
                return [];
            }
            
            // 800k scores is around of 6 hours of missing scores
            var catchUpScoreId = scores.Max(s => s.Id) - 800000;
            _cursor = Convert.ToBase64String(Encoding.Default.GetBytes($"{{\"id\": {catchUpScoreId}}}"));
            _catchUpAfterRestart = false;
            return [];
        }
        
        _catchUpAfterRestart = false;
        _cursor = scoresResponse.Cursor;

        if (scores.Length < 100 && _catchUpOnExistingBeatmapScores)
        {
            _catchUpOnExistingBeatmapScores = false;
        }
        
        var beatmapIds = scores.Select(s => s.BeatmapId).Distinct().ToList();
        var existingBeatmapIds = await dataProcessor.GetBeatmapIdsWithScoresAsync(beatmapIds, stoppingToken);

        var scoresToProcess = scores.Where(s => existingBeatmapIds.Contains(s.BeatmapId)).ToList();
        
        if (scoresToProcess.Count > 0)
        {
            var minDate = scoresToProcess.Min(s => s.Date);
            var maxDate = scoresToProcess.Max(s => s.Date);
            _logger.Log(LogLevel.Information, "Processing a batch of {scoresCount} scores between {minScoreDate} and {maxScoreDate}", 
                scoresToProcess.Count, minDate, maxDate);
        }

        return scoresToProcess;
    }

    /// <summary>
    /// Get existing beatmap scores from the firehose endpoint
    /// </summary>
    /// <param name="scores">List of <see cref="APIScore"/>s</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of <see cref="APIScore"/>s</returns>
    private async Task<List<APIScore>> GetExistingBeatmapScoresAsync(IList<APIScore> scores, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var utils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();
        
        var significantScores = await utils.GetSignificantScoresAsync(scores, stoppingToken);

        return significantScores;
    }

    /// <summary>
    /// Save data from existing beatmap scores
    /// </summary>
    /// <param name="scores">List of <see cref="APIScore"/>s</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task SaveExistingBeatmapScoresAsync(IList<APIScore> scores, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var utils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();
        await utils.SaveScoreDataAsync(scores, ScoreSource.ScoreFetcher, stoppingToken);
    }

    private async Task GetRestartCursorAsync(IDataProcessor dataProcessor, CancellationToken stoppingToken)
    {
        var maxFirehoseScore = await dataProcessor.GetMaxFirehoseScoreAsync(stoppingToken);
        // Score is too old, use null cursor
        if (DateTime.UtcNow - maxFirehoseScore.Date >= TimeSpan.FromDays(1))
        {
            _cursor = null;
            return;
        }
        _cursor = Convert.ToBase64String(Encoding.Default.GetBytes($"{{\"id\": {maxFirehoseScore.Id}}}"));
    }
}