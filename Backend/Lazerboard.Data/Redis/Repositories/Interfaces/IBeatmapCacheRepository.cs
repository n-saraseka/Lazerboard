namespace Lazerboard.Data.Redis.Repositories.Interfaces;

public interface IBeatmapCacheRepository
{
    Task<string?> GetCachedBeatmapFileNameAsync(int beatmapId);
    Task ResetCachedBeatmapFileNameTtlAsync(int beatmapId, TimeSpan ttl);
    Task SetCachedBeatmapFileNameAsync(int beatmapId, string fileName, TimeSpan ttl);
}