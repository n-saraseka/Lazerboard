using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;

namespace Lazerboard.ScoreFetcher.Processing;

public interface IPpBlacklist
{
    Task<bool> CheckIfBlacklistedAsync(int beatmapId, CancellationToken cancellationToken = default);
    Task<Dictionary<int, bool>> CheckIfBlacklistedBulkAsync(IList<int> beatmapIds, CancellationToken cancellationToken = default);
    Task AddToBlacklistAsync(int beatmapId, BlacklistReason reason, CancellationToken cancellationToken = default);
    Task AddToBlacklistBulkAsync(Dictionary<int, BlacklistReason> beatmapsReasons, CancellationToken cancellationToken = default);
}