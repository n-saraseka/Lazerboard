using OsuScoreStats.Calculations;
using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.ScoreFetcher;

public class ScoreProcessor(IScoreRepository scoreRepository, ICalculator calculator) : IScoreProcessor
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
        var scoresForMode = beatmapScores.Where(s => s.Mode == score.Mode).ToList();
        if (scoresForMode.All(s => s.TotalScore > score.TotalScore) && beatmapScores.Count >= 50) 
            return false;
        return !CheckIfBetterAlreadyExists(score, scoresForMode);
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
                var beatmapScores = respectiveGroup.ToList();
                foreach (var score in scoresInGroup)
                {
                    if (beatmapScores.All(s => s.TotalScore > score.TotalScore) && beatmapScores.Count >= 50) 
                        dictionary[score.Id] = false;
                    else 
                        dictionary[score.Id] = !CheckIfBetterAlreadyExists(score, beatmapScores);
                }
            }
            else foreach (var score in scoresInGroup) dictionary[score.Id] = true;
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

    /// <summary>
    /// Check if a better <see cref="Score"/> set by user already exists in the database
    /// </summary>
    /// <param name="score">The <see cref="APIScore"/> to check</param>
    /// <param name="beatmapScores">The <see cref="Score"/>s set on the beatmap</param>
    /// <returns>True if there is a score set by user that is higher or equal, false otherwise</returns>
   public bool CheckIfBetterAlreadyExists(APIScore score, List<Score> beatmapScores)
    {
        var existingScore = beatmapScores.FirstOrDefault(s => s.UserId == score.UserId);
        if (existingScore == null) return false;
        return existingScore.TotalScore >= score.TotalScore;
    }
}