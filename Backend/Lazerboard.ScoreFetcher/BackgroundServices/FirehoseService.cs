using System.Text;
using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Lazerboard.ScoreFetcher.Processing;

namespace Lazerboard.ScoreFetcher.BackgroundServices;

public class FirehoseService(IServiceProvider serviceProvider, ILogger<BeatmapsetUpdatesService> logger)
    : BackgroundService
{
    private bool _catchUpOnExistingBeatmapScores;
    private string? _cursor;
    private int _insertedCount;
    private int _repeatExponent;
    private readonly int _baseRepeatSeconds = 30;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await GetRestartCursorAsync(stoppingToken);
        logger.Log(LogLevel.Information, "Restart cursor for the firehose: {cursor}", _cursor);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var scores = await FetchExistingBeatmapScoresAsync(stoppingToken);
                if (scores.Count > 0)
                {
                    var significantScores = await GetExistingBeatmapScoresAsync(scores, stoppingToken);
                    var scoresWithoutPp = significantScores.Where(s => s.PP == null).ToList();
                    var scoresWithPp = significantScores.Where(s => s.PP != null).ToList();

                    var groupedByBeatmapId = scoresWithoutPp.GroupBy(s => s.BeatmapId).ToList();
                    foreach (var group in groupedByBeatmapId)
                    {
                        using var scope = serviceProvider.CreateScope();
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
                    _insertedCount += await SaveExistingBeatmapScoresAsync(mergedScores, stoppingToken);
                }
                if (_catchUpOnExistingBeatmapScores) continue;
                if (_insertedCount == 0)
                {
                    _repeatExponent = Math.Min(_repeatExponent + 1, 5);
                    logger.Log(LogLevel.Information, "No new scores inserted. Repeating in {interval} seconds", 
                        _baseRepeatSeconds * Math.Pow(2, _repeatExponent));
                }
                else
                {
                    logger.Log(LogLevel.Information, "Inserted {count} new scores", _insertedCount);
                    _repeatExponent = 0;
                    _insertedCount = 0;
                }
                await Task.Delay(TimeSpan.FromSeconds(_baseRepeatSeconds * Math.Pow(2, _repeatExponent)), stoppingToken);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Critical, ex, "Firehose service failed!");
            }
        }
    }

    /// <summary>
    /// Get scores from the firehose endpoint and filter them to ones from maps that are already in the database
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task<List<APIScore>> FetchExistingBeatmapScoresAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
        var apiFetcher = scope.ServiceProvider.GetRequiredService<IOsuApiFetcher>();
        _catchUpOnExistingBeatmapScores = true;
        
        var scoresResponse = await apiFetcher.GetScoresAsync(_cursor, stoppingToken);
        var scores = scoresResponse.Scores;
        
        if (_cursor is null)
        {
            if (scores.Length == 0)
            {
                logger.Log(LogLevel.Warning, "Couldn't get max score ID from firehose!");
                return [];
            }
            
            // 800k scores is around of 6 hours of missing scores
            var catchUpScoreId = scores.Max(s => s.Id) - 800000;
            _cursor = Convert.ToBase64String(Encoding.Default.GetBytes($"{{\"id\": {catchUpScoreId}}}"));
            return [];
        }
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
            logger.Log(LogLevel.Information, "Processing a batch of {scoresCount} scores between {minScoreDate} and {maxScoreDate}", 
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
        using var scope = serviceProvider.CreateScope();
        var utils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();
        
        var significantScores = await utils.GetSignificantScoresAsync(scores, stoppingToken);

        return significantScores;
    }

    /// <summary>
    /// Save data from existing beatmap scores
    /// </summary>
    /// <param name="scores">List of <see cref="APIScore"/>s</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task<int> SaveExistingBeatmapScoresAsync(IList<APIScore> scores, CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var utils = scope.ServiceProvider.GetRequiredService<IScoreFetchingUtils>();
        return await utils.SaveScoreDataAsync(scores, ScoreSource.ScoreFetcher, stoppingToken);
    }

    /// <summary>
    /// Get the cursor string for firehose restart
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    private async Task GetRestartCursorAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dataProcessor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
        var maxFirehoseScore = await dataProcessor.GetMaxFirehoseScoreAsync(stoppingToken);
        // Score is too old, use null cursor
        if (DateTime.UtcNow - maxFirehoseScore.Date >= TimeSpan.FromHours(6))
        {
            return;
        }
        _cursor = Convert.ToBase64String(Encoding.Default.GetBytes($"{{\"id\": {maxFirehoseScore.Id}}}"));
    }
}