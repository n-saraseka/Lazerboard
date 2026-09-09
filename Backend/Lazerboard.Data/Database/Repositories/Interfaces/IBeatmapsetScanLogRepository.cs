using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IBeatmapsetScanLogRepository
{
    Task<BeatmapsetScanLog?> GetLatestStartedScanAsync(CancellationToken cancellationToken = default);
    Task<BeatmapsetScanLog?> GetLatestFinishedScanAsync(CancellationToken cancellationToken = default);
    Task<BeatmapsetScanLog?> GetLatestStartedMainSeedingAsync(CancellationToken cancellationToken = default);
    Task<BeatmapsetScanLog?> GetLatestFinishedMainSeedingAsync(CancellationToken cancellationToken = default);
    Task<BeatmapsetScanLog?> GetLatestStartedSecondarySeedingAsync(CancellationToken cancellationToken = default);
    Task<BeatmapsetScanLog?> GetLatestFinishedSecondarySeedingAsync(CancellationToken cancellationToken = default);
    Task<int> SaveEventAsync(ScanEventType type, CancellationToken cancellationToken = default);
}