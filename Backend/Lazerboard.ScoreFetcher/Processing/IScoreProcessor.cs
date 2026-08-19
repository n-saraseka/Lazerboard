using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.OsuEntities.OsuApiEntities;

namespace Lazerboard.ScoreFetcher.Processing;

public interface IScoreProcessor
{
    Task<bool> CheckIfSignificantAsync(APIScore score, CancellationToken cancellationToken);
    Task<Dictionary<ulong, bool>> CheckIfSignificantBulkAsync(IEnumerable<APIScore> scores, CancellationToken cancellationToken);
    bool CheckIfBetterAlreadyExists(APIScore score, List<Score> beatmapScores);
    Task CalculateScoreAsync(APIScore score, CancellationToken cancellationToken);
}