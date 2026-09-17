using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Data.Database.Repositories;

public class UserScanLogRepository(ScoreDataContext db) : BaseRepository<UserScanLog, int>(db), IUserScanLogRepository
{
    /// <summary>
    /// Get the latest <see cref="UserScanLog"/>
    /// where the <see cref="UserScanLog.EventType"/> is <see cref="ScanEventType.UserScanStarted"/>.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="UserScanLog"/> or null</returns>
    public Task<UserScanLog?> GetLatestStartedScanAsync(CancellationToken cancellationToken = default) =>
        Set
            .Where(b => b.EventType == ScanEventType.UserScanStarted)
            .OrderByDescending(b => b.LoggedAt)
            .FirstOrDefaultAsync(cancellationToken);
    
    /// <summary>
    /// Get the latest <see cref="UserScanLog"/>
    /// where the <see cref="UserScanLog.EventType"/> is <see cref="ScanEventType.UserScanFinished"/>.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="UserScanLog"/> or null</returns>
    public Task<UserScanLog?> GetLatestFinishedScanAsync(CancellationToken cancellationToken = default) =>
        Set
            .Where(b => b.EventType == ScanEventType.UserScanFinished)
            .OrderByDescending(b => b.LoggedAt)
            .FirstOrDefaultAsync(cancellationToken);
}