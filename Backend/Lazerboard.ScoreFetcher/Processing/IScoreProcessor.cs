using Lazerboard.Data.OsuEntities.OsuApiEntities;
using osu.Game.Beatmaps;

namespace Lazerboard.ScoreFetcher.Processing;

public interface IScoreProcessor
{
    Task<bool> CheckIfSignificantAsync(APIScore score, CancellationToken cancellationToken);
    Task<Dictionary<ulong, bool>> CheckIfSignificantBulkAsync(IEnumerable<APIScore> scores, CancellationToken cancellationToken);
    Task CalculateScoreAsync(APIScore score, FlatWorkingBeatmap flatWorkingBeatmap, CancellationToken cancellationToken);
}