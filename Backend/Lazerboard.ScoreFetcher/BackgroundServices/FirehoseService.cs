using System.Text;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
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
    private bool _catchUpAfterRestart;
    private bool _catchUpOnExistingBeatmapScores;

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
        
        var restartConfig = config.GetSection("RestartPolicy");
        _catchUpAfterRestart = bool.Parse(restartConfig["FirehoseCatchUp"]);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_seedingState.IsSeeding)
                {
                    var scores = await FetchExistingBeatmapScoresAsync(stoppingToken);
                    if (scores.Count > 0)
                    {
                        var significantScores = await GetExistingBeatmapScoresAsync(scores, stoppingToken);
                        var scoresWithoutPp = significantScores.Where(s => s.PP == null).ToList();
                        var scoresWithPp = significantScores.Where(s => s.PP != null).ToList();
                        
                        foreach (var score in scoresWithoutPp)
                        {
                            await CalculateScorePpAsync(score, stoppingToken);
                        }
                        
                        var mergedScores = scoresWithPp.Concat(scoresWithoutPp).ToList();
                        await SaveExistingBeatmapScoresAsync(mergedScores, stoppingToken);
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    }
                    if (!_catchUpOnExistingBeatmapScores)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                    }
                }
                else
                {
                    var scores = await FetchFromFirehoseAsync(stoppingToken);
                    if (scores.Length == 0) continue;
                    
                    var significantScores = await GetFirehoseScoresAsync(scores, stoppingToken);
                    var scoresWithoutPp = significantScores.Where(s => s.PP == null).ToList();
                    var scoresWithPp = significantScores.Where(s => s.PP != null).ToList();

                    foreach (var score in scoresWithoutPp)
                    {
                        await CalculateScorePpAsync(score, stoppingToken);
                    }
                    
                    var mergedScores = scoresWithPp.Concat(scoresWithoutPp).ToList();
                    await SaveFirehoseDataAsync(mergedScores, stoppingToken);
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
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IApiFetcher>();
        
        if (_catchUpAfterRestart)
        {
            await GetRestartCursorAsync(dataProcessor, stoppingToken);
        }
        
        _catchUpOnExistingBeatmapScores = true;
        
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

        if (scores.Length < 100)
        {
            _catchUpOnExistingBeatmapScores = false;
            return [];
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
        var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
        var utils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();
        
        await utils.SaveUserDataFromScoresAsync(scores,  stoppingToken);
        await dataProcessor.ProcessScoresAsync(scores, ScoreSource.ScoreFetcher, stoppingToken);
    }
    
    /// <summary>
    /// Get scores from the firehose endpoint
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task<APIScore[]> FetchFromFirehoseAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IApiFetcher>();
        
        if (_catchUpAfterRestart)
        {
            await GetRestartCursorAsync(dataProcessor, stoppingToken);
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

        var minDate = scores.Min(s => s.Date);
        var maxDate = scores.Max(s => s.Date);
        _logger.Log(LogLevel.Information, "Processing a batch of {scoresCount} scores between {minScoreDate} and {maxScoreDate}", 
            scores.Length, minDate, maxDate);

        return scores;
    }

    /// <summary>
    /// Get significant <see cref="APIScore"/>s from the general firehose
    /// </summary>
    /// <param name="scores">List of <see cref="APIScore"/>s</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>List of significant <see cref="APIScore"/>s</returns>
    private async Task<List<APIScore>> GetFirehoseScoresAsync(IList<APIScore> scores, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var utils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();
        
        var significantScores = await utils.GetSignificantScoresAsync(scores, stoppingToken);
        
        if (significantScores.Count == 0)
        {
            _logger.Log(LogLevel.Information, "No significant scores found after {interval} seconds. Repeating in {nextInterval} seconds", 
                _apiInterval * Math.Pow(2, _repeatExponent), _apiInterval * Math.Pow(2, _repeatExponent + 1));
            _repeatExponent++;
            var interval = _apiInterval * Math.Pow(2, _repeatExponent);
            await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
            return [];
        }
        _repeatExponent = 0;
        return significantScores;
    }

    /// <summary>
    /// Save score data from the general firehose
    /// </summary>
    /// <param name="scores">List of <see cref="APIScore"/>s</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task SaveFirehoseDataAsync(IList<APIScore> scores, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var utils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();
        var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IApiFetcher>();
            
        // Process new beatmaps and beatmapsets first if necessary
        var beatmapIds = scores.Select(s => s.BeatmapId).Distinct().ToList();
        var existingBeatmaps = await dataProcessor.GetExistingBeatmapsAsync(beatmapIds, stoppingToken);
        var newBeatmapIds = beatmapIds.Where(id => !existingBeatmaps.Select(b => b.Id).Contains(id)).ToList();

        if (newBeatmapIds.Count > 0)
        {
            var beatmaps = await apiFetcher.GetBeatmapsAsync(newBeatmapIds, stoppingToken);
            var beatmapsets = beatmaps.Select(b => b.Beatmapset).Distinct().ToList();
            await utils.SaveAllBeatmapsetDataAsync(beatmapsets, stoppingToken);
            await dataProcessor.ProcessBeatmapsAsync(beatmaps, stoppingToken);
        }
            
        await SaveExistingBeatmapScoresAsync(scores, stoppingToken);
    }
    
    /// <summary>
    /// Calculate <see cref="APIScore"/>'s PP value and save it to the <see cref="APIScore.PP"/> field
    /// </summary>
    /// <param name="score">The <see cref="APIScore"/></param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task CalculateScorePpAsync(APIScore score, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scoreProcessor = scope.ServiceProvider.GetRequiredService<IScoreProcessor>();
        
        await scoreProcessor.CalculateScoreAsync(score, stoppingToken);
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