using Lazerboard.Data.Redis.Repositories.Interfaces;
using Lazerboard.ScoreFetcher.Processing;

namespace Lazerboard.ScoreFetcher.Calculations;

public interface ICacheStore
{
    Task<string> GetBeatmapFileStringAsync(int beatmapId, 
        IBeatmapCacheRepository beatmapCacheRepository, 
        CancellationToken ct);
    Task CleanupCacheAsync();
}