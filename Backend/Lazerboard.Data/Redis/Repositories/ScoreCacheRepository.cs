using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.Redis.Repositories.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Lazerboard.Data.Redis.Repositories;

public class ScoreCacheRepository (IConnectionMultiplexer connectionMultiplexer) : IScoreCacheRepository
{
    public async Task<bool?> GetScoreCalculatableAsync(int beatmapId, Mode mode)
    {
        var key = $"{beatmapId}:{mode}-iscalculatable";
        var db = connectionMultiplexer.GetDatabase();
        var value = await db.StringGetAsync(key);
        return (bool?)value;
    }

    public async Task<bool> SetScoreCalculatableAsync(int beatmapId, Mode mode, bool isCalculatable)
    {
        var key = $"{beatmapId}:{mode}-iscalculatable";
        var db = connectionMultiplexer.GetDatabase();
        await db.StringSetAsync(key, isCalculatable, TimeSpan.FromMinutes(2), false, When.NotExists);
        var value = await db.StringGetAsync(key);
        return (bool)value;
    }

    public async Task<int?> GetAllScoresCountAsync()
    {
        var key = "scores-count";
        var db = connectionMultiplexer.GetDatabase();
        var value = await db.StringGetAsync(key);
        return (int?)value;
    }

    public async Task<int> SetAllScoresCountAsync(int scoreCount, TimeSpan ttl)
    {
        var key = "scores-count";
        var db = connectionMultiplexer.GetDatabase();
        var value = await db.StringSetAndGetAsync(key, scoreCount, ttl);
        return (int)value;
    }

    public async Task<string?> GetSerializedResponseAsync(string key)
    {
        var db = connectionMultiplexer.GetDatabase();
        return await db.StringGetAsync(key);
    }
    
    public async Task SetSerializedResponseAsync(string key, string serializedResponse, TimeSpan ttl)
    {
        var db = connectionMultiplexer.GetDatabase();
        await db.StringSetAsync(key, serializedResponse, ttl);
    }
}