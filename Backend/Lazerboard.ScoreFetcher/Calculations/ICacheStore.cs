using Lazerboard.Data.Redis.Repositories.Interfaces;
using osu.Game.Beatmaps;

namespace Lazerboard.ScoreFetcher.Calculations;

public interface ICacheStore
{
    Task<Beatmap> GetBeatmapFileAsync(int beatmapId, IBeatmapCacheRepository beatmapCacheRepository, CancellationToken ct);
    Task CleanupCacheAsync();
}