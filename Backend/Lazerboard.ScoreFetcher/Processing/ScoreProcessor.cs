using Microsoft.Extensions.Logging;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Lazerboard.ScoreFetcher.Calculations;

namespace Lazerboard.ScoreFetcher.Processing;

public class ScoreProcessor(IScoreRepository scoreRepository, ICalculator calculator, ILogger<IScoreProcessor> logger) : IScoreProcessor
{
    /// <summary>
    /// Check if a score is significant (higher than the min TotalScore and no better score set by user exists)
    /// </summary>
    /// <param name="score">The <see cref="APIScore"/></param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>True if score is deemed significant, false otherwise</returns>
    public async Task<bool> CheckIfSignificantAsync(APIScore score, CancellationToken cancellationToken)
    {
        var beatmapScores = await scoreRepository.GetByBeatmapIdAsync(score.BeatmapId, cancellationToken);
        var scoresForMode = 
            beatmapScores
                .Where(s => s.Mode == score.Mode)
                .OrderByDescending(s => s.TotalScore)
                .ThenBy(s => s.Date)
                .ToList();
        if (scoresForMode.Count == 0) return true;
        var lastScore = scoresForMode.Last();
        return !((lastScore.TotalScore > score.TotalScore 
                  || lastScore.TotalScore == score.TotalScore && lastScore.Date < score.Date)
                 && scoresForMode.Count >= 100);
    }
    
    /// <summary>
    /// Check if multiple scores are significant (higher than the min TotalScore and no better score set by user exists)
    /// </summary>
    /// <param name="scores">The <see cref="APIScore"/>s</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A dictionary of results of checks for every score ID</returns>
    public async Task<Dictionary<ulong, bool>> CheckIfSignificantBulkAsync(IEnumerable<APIScore> scores, CancellationToken cancellationToken)
    {
        var dictionary = new Dictionary<ulong, bool>();
        var groupedByBeatmapId = scores.GroupBy(s => new { s.BeatmapId, s.Mode }).ToList();
        var beatmapIds = scores.Select(s => s.BeatmapId).Distinct();
        
        var existingScores = await scoreRepository.GetByBeatmapIdsAsync(beatmapIds, cancellationToken);
        var groupedExistingScores = existingScores.GroupBy(s => new { s.BeatmapId, s.Mode }).ToList(); 
        
        foreach (var group in groupedByBeatmapId)
        {
            var scoresInGroup = group.ToList();
            
            var respectiveGroup = groupedExistingScores.FirstOrDefault(g => 
                g.Key.Mode == group.Key.Mode && g.Key.BeatmapId == group.Key.BeatmapId);
            if (respectiveGroup != null)
            {
                var beatmapScores = respectiveGroup
                    .OrderByDescending(s => s.TotalScore)
                    .ThenBy(s => s.Date)
                    .ToList();
                var lastScore = beatmapScores.Last();
                foreach (var score in scoresInGroup)
                {
                    dictionary[score.Id] = !((lastScore.TotalScore > score.TotalScore 
                                              || lastScore.TotalScore == score.TotalScore && lastScore.Date < score.Date)
                                             && beatmapScores.Count >= 100);
                }
            }
            else foreach (var score in scoresInGroup) dictionary[score.Id] = true;
        }

        if (dictionary.Any(kvp => kvp.Value))
        {
            logger.Log(LogLevel.Information, "Significant score IDs: {@scoreIds}", 
                dictionary.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList());
        }
        
        return dictionary;
    }

    /// <summary>
    /// Calculate PP for a score
    /// </summary>
    /// <param name="score">The <see cref="APIScore"/></param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    public async Task CalculateScoreAsync(APIScore score, CancellationToken cancellationToken)
    {
        if (score.PP != null) return;
        score.PP = await calculator.CalculateAsync(score, cancellationToken);
    }
}