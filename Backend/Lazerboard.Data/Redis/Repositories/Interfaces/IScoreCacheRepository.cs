using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.OsuEntities.Enums;

namespace Lazerboard.Data.Redis.Repositories.Interfaces;

public interface IScoreCacheRepository
{
    Task<bool?> GetScoreCalculatableAsync(int beatmapId, Mode mode);
    Task<bool> SetScoreCalculatableAsync(int beatmapId, Mode mode, bool isCalculatable);
    Task<int?> GetAllScoresCountAsync();
    Task<int> SetAllScoresCountAsync(int scoreCount, TimeSpan ttl);
    Task<List<Score>?> GetScoresAsync(string key);
    Task SetScoresAsync(string key, List<Score> scores, TimeSpan ttl);
    Task<int?> GetScoresCountAsync(string key);
    Task SetScoresCountAsync(string key, int scoreCount, TimeSpan ttl);
}