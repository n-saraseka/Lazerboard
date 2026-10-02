using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using osu.Game.Beatmaps;

namespace Lazerboard.ScoreFetcher.Processing;

public interface IBeatmapUtils
{
    Task ProcessBeatmapsetAsync(APIBeatmapset beatmapset, ScanEventType eventType, CancellationToken stoppingToken);
    Task ProcessLeaderboardAsync(int beatmapId, Mode mode, CancellationToken stoppingToken);
    Task ProcessExistingMapsetAsync(Beatmapset beatmapset, ScanEventType eventType, CancellationToken stoppingToken);
    Task SaveStartingTimestampAsync(IList<int> beatmapsetIds, ScanEventType eventType, CancellationToken stoppingToken);
    Task SaveFinishingTimestampAsync(IList<int> beatmapsetIds, ScanEventType eventType,
        CancellationToken stoppingToken);
    Task SaveBeatmapScansTimestampsAsync(IList<int> beatmapIds, CancellationToken stoppingToken);
}