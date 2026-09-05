using Lazerboard.Data.Redis.Repositories.Interfaces;
using StackExchange.Redis;

namespace Lazerboard.Data.Redis.Repositories;

public class BeatmapCacheRepository(RedisContext context) : IBeatmapCacheRepository
{
    public async Task<string?> GetCachedBeatmapFileNameAsync(int beatmapId)
    {
        var key = $"{beatmapId}-beatmapfile";
        var db = context.ConnectionMultiplexer.GetDatabase();
        var value = await db.StringGetAsync(key);
        return value;
    }

    public async Task ResetCachedBeatmapFileNameTtlAsync(int beatmapId, TimeSpan ttl)
    {
        var key = $"{beatmapId}-beatmapfile";
        var db = context.ConnectionMultiplexer.GetDatabase();
        await db.KeyExpireAsync(key, ttl);
    }

    public async Task SetCachedBeatmapFileNameAsync(int beatmapId, string fileName, TimeSpan ttl)
    {
        var key = $"{beatmapId}-beatmapfile";
        var db = context.ConnectionMultiplexer.GetDatabase();
        
        await db.StringSetAsync(key, fileName, ttl, false);
    }
}