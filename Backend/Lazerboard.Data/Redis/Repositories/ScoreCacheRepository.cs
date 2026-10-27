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

    public async Task<List<Score>?> GetScoresAsync(string key)
    {
        var db = connectionMultiplexer.GetDatabase();
        var stringValue = await db.StringGetAsync(key);
        if (!stringValue.HasValue) return null;
        var listValue = JsonConvert.DeserializeObject<List<Score>>(stringValue!, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        return listValue;
    }
    
    public async Task SetScoresAsync(string key, List<Score> scores, TimeSpan ttl)
    {
        var db = connectionMultiplexer.GetDatabase();
        var serializedValue = JsonConvert.SerializeObject(scores);
        await db.StringSetAsync(key, serializedValue, ttl);
    }

    public async Task<int?> GetScoresCountAsync(string key)
    {
        var db = connectionMultiplexer.GetDatabase();
        var value = await db.StringGetAsync(key);
        return (int?)value;
    }

    public async Task SetScoresCountAsync(string key, int scoreCount, TimeSpan ttl)
    {
        var db = connectionMultiplexer.GetDatabase();
        await db.StringSetAsync(key, scoreCount, ttl);
    }
}