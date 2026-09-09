using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using osu.Game.Beatmaps;

namespace Lazerboard.ScoreFetcher.Processing;

public interface IScoreFetchingUtils
{
    Task SaveAllBeatmapsetDataAsync(IList<APIBeatmapset> beatmapsets, ScanEventType eventType, CancellationToken stoppingToken);
    Task<List<APIScore>> GetSignificantScoresAsync(IList<APIScore> scores, CancellationToken stoppingToken);
    Task SaveUserDataFromScoresAsync(IList<APIScore> scores, CancellationToken stoppingToken);
    Task SaveScoreDataAsync(IList<APIScore> scores, ScoreSource source, CancellationToken stoppingToken);
    Task<FlatWorkingBeatmap> GetFlatWorkingBeatmapAsync(int beatmapId, CancellationToken stoppingToken);
}