using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IBeatmapsetScanLogRepository
{
    Task<BeatmapsetScanLog?> GetLatestStartedScanAsync(CancellationToken cancellationToken = default);
    Task<BeatmapsetScanLog?> GetLatestFinishedScanAsync(CancellationToken cancellationToken = default);
    Task<BeatmapsetScanLog?> GetLatestStartedMainSeedingAsync(CancellationToken cancellationToken = default);
    Task<BeatmapsetScanLog?> GetLatestFinishedMainSeedingAsync(CancellationToken cancellationToken = default);
    Task<BeatmapsetScanLog?> GetLatestStartedSecondarySeedingAsync(CancellationToken cancellationToken = default);
    Task<BeatmapsetScanLog?> GetLatestFinishedSecondarySeedingAsync(CancellationToken cancellationToken = default);
}