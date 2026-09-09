using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using osu.Game.Beatmaps;

namespace Lazerboard.ScoreFetcher.Processing;

public interface IBeatmapUtils
{
    Task<List<APIScore>> GetBeatmapScoresAsync(int beatmapId, Mode mode, CancellationToken stoppingToken);
    Task<FlatWorkingBeatmap> GetFlatWorkingBeatmapAsync(int beatmapId, CancellationToken stoppingToken);
}