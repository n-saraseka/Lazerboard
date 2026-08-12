using OsuScoreStats.Shared.DbService.Entities;
using OsuScoreStats.Shared.OsuApi.OsuApiEntities;

namespace OsuScoreStats.Shared.Processing;

public interface IScoreProcessor
{
    Task<bool> CheckIfSignificantAsync(APIScore score, CancellationToken cancellationToken);
    Task<Dictionary<ulong, bool>> CheckIfSignificantBulkAsync(IList<APIScore> scores, CancellationToken cancellationToken);
    bool CheckIfBetterAlreadyExists(APIScore score, List<Score> beatmapScores);
    Task CalculateScoreAsync(APIScore score, CancellationToken cancellationToken);
}