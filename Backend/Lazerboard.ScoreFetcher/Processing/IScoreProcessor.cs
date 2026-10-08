using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using osu.Game.Beatmaps;

namespace Lazerboard.ScoreFetcher.Processing;

public interface IScoreProcessor
{
    Task<bool> CheckIfSignificantAsync(APIScore score, CancellationToken cancellationToken);
    Task<Dictionary<ulong, bool>> CheckIfSignificantBulkAsync(IList<APIScore> scores, CancellationToken cancellationToken);
    Task<Dictionary<ulong, bool>> CheckIfSignificantBulkAsync(IList<Score> scores, CancellationToken cancellationToken);
    Task CalculateScoreAsync(APIScore score, FlatWorkingBeatmap flatWorkingBeatmap, CancellationToken cancellationToken);
}