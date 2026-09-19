using Lazerboard.Data.Database.Entities;

namespace Lazerboard.ScoreFetcher.Processing;

public interface IUserUtils
{
    Task ProcessExistingUsersAsync(IList<User> users, bool isUserScan, CancellationToken stoppingToken);
    Task ProcessRestrictedUsersAsync(IList<User> users, CancellationToken stoppingToken);
}