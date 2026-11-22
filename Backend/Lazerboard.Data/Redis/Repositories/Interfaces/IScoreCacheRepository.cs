using Lazerboard.Data.OsuEntities.Enums;

namespace Lazerboard.Data.Redis.Repositories.Interfaces;

public interface IScoreCacheRepository
{
    Task<bool?> GetScoreCalculatableAsync(int beatmapId, Mode mode);
    Task<bool> SetScoreCalculatableAsync(int beatmapId, Mode mode, bool isCalculatable);
    Task<string?> GetSerializedResponseAsync(string key);
    Task SetSerializedResponseAsync(string key, string serializedResponse, TimeSpan ttl);
}