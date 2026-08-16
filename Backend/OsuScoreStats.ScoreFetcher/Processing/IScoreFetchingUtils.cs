using OsuScoreStats.Data.OsuEntities.OsuApiEntities;

namespace OsuScoreStats.ScoreFetcher.Processing;

public interface IScoreFetchingUtils
{
    Task SaveAllBeatmapsetDataAsync(IReadOnlyCollection<APIBeatmapset> beatmapsets, CancellationToken stoppingToken);
    Task<List<APIScore>> GetSignificantScoresAsync(IList<APIScore> scores, CancellationToken stoppingToken);
    Task SaveUserDataFromScoresAsync(IList<APIScore> scores, CancellationToken stoppingToken);
}