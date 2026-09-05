using Lazerboard.Data.Redis.Repositories.Interfaces;
using StackExchange.Redis;

namespace Lazerboard.Data.Redis.Repositories;

public class BeatmapCacheRepository(IConnectionMultiplexer connectionMultiplexer) : IBeatmapCacheRepository
{
    public async Task<string?> GetCachedBeatmapFileNameAsync(int beatmapId)
    {
        var key = $"{beatmapId}-beatmapfile";
        var db = connectionMultiplexer.GetDatabase();
        var value = await db.StringGetAsync(key);
        return value;
    }

    public async Task<Dictionary<int, string?>> GetCachedBeatmapFileNamesAsync(IList<int> beatmapIds)
    {
        var keys = beatmapIds.Select(beatmapId => (RedisKey)$"{beatmapId}-beatmapfile").ToArray();
        var db = connectionMultiplexer.GetDatabase();
        var values = await db.StringGetAsync(keys);
        
        var dict = new Dictionary<int, string?>();

        for (var i = 0; i < beatmapIds.Count; i++)
        {
            dict[beatmapIds[i]] = values[i];
        }

        return dict;
    }

    public async Task ResetCachedBeatmapFileNameTtlAsync(int beatmapId, TimeSpan ttl)
    {
        var key = $"{beatmapId}-beatmapfile";
        var db = connectionMultiplexer.GetDatabase();
        await db.KeyExpireAsync(key, ttl);
    }

    public async Task SetCachedBeatmapFileNameAsync(int beatmapId, string fileName, TimeSpan ttl)
    {
        var key = $"{beatmapId}-beatmapfile";
        var db = connectionMultiplexer.GetDatabase();
        
        await db.StringSetAsync(key, fileName, ttl, false);
    }
}