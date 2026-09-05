using Lazerboard.Data.OsuEntities.Enums;

namespace Lazerboard.Data.Redis.Repositories.Interfaces;

public interface IScoreCacheRepository
{
    Task<bool?> GetScoreCalculatableAsync(int beatmapId, Mode mode);
    Task<bool> SetScoreCalculatableAsync(int beatmapId, Mode mode, bool isCalculatable);
}