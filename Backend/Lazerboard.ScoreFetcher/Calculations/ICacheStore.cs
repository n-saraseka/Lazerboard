using osu.Game.Beatmaps;

namespace Lazerboard.ScoreFetcher.Calculations;

public interface ICacheStore
{
    Task<Beatmap> GetBeatmapFileAsync(int beatmapId, CancellationToken ct);
    Task CleanupCacheAsync();
}