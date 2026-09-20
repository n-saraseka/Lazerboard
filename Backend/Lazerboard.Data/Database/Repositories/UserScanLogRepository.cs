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
    
    /// <summary>
    /// Get the latest <see cref="UserScanLog"/>
    /// where the <see cref="UserScanLog.EventType"/> is <see cref="ScanEventType.UserCheckStarted"/>.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="UserScanLog"/> or null</returns>
    public Task<UserScanLog?> GetLatestStartedCheckAsync(CancellationToken cancellationToken = default) =>
        Set
            .Where(b => b.EventType == ScanEventType.UserCheckStarted)
            .OrderByDescending(b => b.LoggedAt)
            .FirstOrDefaultAsync(cancellationToken);
    
    /// <summary>
    /// Get the latest <see cref="UserScanLog"/>
    /// where the <see cref="UserScanLog.EventType"/> is <see cref="ScanEventType.UserCheckFinished"/>.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="UserScanLog"/> or null</returns>
    public Task<UserScanLog?> GetLatestFinishedCheckAsync(CancellationToken cancellationToken = default) =>
        Set
            .Where(b => b.EventType == ScanEventType.UserCheckFinished)
            .OrderByDescending(b => b.LoggedAt)
            .FirstOrDefaultAsync(cancellationToken);
    
    /// <summary>
    /// Save a new <see cref="UserScanLog"/> event with current timestamp
    /// </summary>
    /// <param name="type">The <see cref="ScanEventType"/></param>
    /// <param name="dateTime">The <see cref="DateTime"/></param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>Number of rows inserted into the DB</returns>
    public async Task<int> SaveEventAsync(ScanEventType type, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        var newRow = new UserScanLog
        {
            EventType = type,
            LoggedAt = dateTime
        };
        
        Create(newRow);
        return await SaveChangesAsync(cancellationToken);
    }
}