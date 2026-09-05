using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.Redis.Repositories.Interfaces;
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

        var success = false;
        while (!success)
        {
            success = await db.StringSetAsync(key, isCalculatable, TimeSpan.FromMinutes(2), false, When.NotExists);
        }
        
        var value = await db.StringGetAsync(key);
        
        return (bool)value;
    }
}