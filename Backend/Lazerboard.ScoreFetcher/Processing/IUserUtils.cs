using Lazerboard.Data.Database.Entities;

namespace Lazerboard.ScoreFetcher.Processing;

public interface IUserUtils
{
    Task ProcessUsersAsync(IList<User> users, bool isUserScan, CancellationToken stoppingToken);
    Task<int> RemoveUserScoresAsync(IList<int> userIds, CancellationToken stoppingToken);
    Task ReprocessBeatmapRanksAsync(IList<int> beatmapIds, CancellationToken stoppingToken);
}