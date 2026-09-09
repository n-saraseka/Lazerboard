using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Data.Database.Repositories;

public class BeatmapsetScanLogRepository(ScoreDataContext db) : BaseRepository<BeatmapsetScanLog, int>(db), IBeatmapsetScanLogRepository
{
    /// <summary>
    /// Get the latest <see cref="BeatmapsetScanLog"/>
    /// where the <see cref="BeatmapsetScanLog.EventType"/> is <see cref="ScanEventType.RescanStarted"/>.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="BeatmapsetScanLog"/> or null</returns>
    public Task<BeatmapsetScanLog?> GetLatestStartedScanAsync(CancellationToken cancellationToken = default) =>
        Set
            .Where(b => b.EventType == ScanEventType.RescanStarted)
            .OrderByDescending(b => b.LoggedAt)
            .FirstOrDefaultAsync(cancellationToken);
    
    /// <summary>
    /// Get the latest <see cref="BeatmapsetScanLog"/>
    /// where the <see cref="BeatmapsetScanLog.EventType"/> is <see cref="ScanEventType.RescanFinished"/>.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="BeatmapsetScanLog"/> or null</returns>
    public Task<BeatmapsetScanLog?> GetLatestFinishedScanAsync(CancellationToken cancellationToken = default) =>
        Set
            .Where(b => b.EventType == ScanEventType.RescanFinished)
            .OrderByDescending(b => b.LoggedAt)
            .FirstOrDefaultAsync(cancellationToken);
    
    /// <summary>
    /// Get the latest <see cref="BeatmapsetScanLog"/>
    /// where the <see cref="BeatmapsetScanLog.EventType"/> is <see cref="ScanEventType.MainSeedingStarted"/>.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="BeatmapsetScanLog"/> or null</returns>
    public Task<BeatmapsetScanLog?> GetLatestStartedMainSeedingAsync(CancellationToken cancellationToken = default) =>
        Set
            .Where(b => b.EventType == ScanEventType.MainSeedingStarted)
            .OrderByDescending(b => b.LoggedAt)
            .FirstOrDefaultAsync(cancellationToken);
    
    /// <summary>
    /// Get the latest <see cref="BeatmapsetScanLog"/>
    /// where the <see cref="BeatmapsetScanLog.EventType"/> is <see cref="ScanEventType.MainSeedingFinished"/>.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="BeatmapsetScanLog"/> or null</returns>
    public Task<BeatmapsetScanLog?> GetLatestFinishedMainSeedingAsync(CancellationToken cancellationToken = default) =>
        Set
            .Where(b => b.EventType == ScanEventType.MainSeedingFinished)
            .OrderByDescending(b => b.LoggedAt)
            .FirstOrDefaultAsync(cancellationToken);
    
    /// <summary>
    /// Get the latest <see cref="BeatmapsetScanLog"/>
    /// where the <see cref="BeatmapsetScanLog.EventType"/> is <see cref="ScanEventType.SecondarySeedingStarted"/>.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="BeatmapsetScanLog"/> or null</returns>
    public Task<BeatmapsetScanLog?> GetLatestStartedSecondarySeedingAsync(CancellationToken cancellationToken = default) =>
        Set
            .Where(b => b.EventType == ScanEventType.SecondarySeedingStarted)
            .OrderByDescending(b => b.LoggedAt)
            .FirstOrDefaultAsync(cancellationToken);
    
    /// <summary>
    /// Get the latest <see cref="BeatmapsetScanLog"/>
    /// where the <see cref="BeatmapsetScanLog.EventType"/> is <see cref="ScanEventType.SecondarySeedingFinished"/>.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="BeatmapsetScanLog"/> or null</returns>
    public Task<BeatmapsetScanLog?> GetLatestFinishedSecondarySeedingAsync(CancellationToken cancellationToken = default) =>
        Set
            .Where(b => b.EventType == ScanEventType.SecondarySeedingFinished)
            .OrderByDescending(b => b.LoggedAt)
            .FirstOrDefaultAsync(cancellationToken);
}