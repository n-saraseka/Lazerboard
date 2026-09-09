using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IBeatmapsetRepository : IRepository<Beatmapset, int>
{
    Task<Beatmapset?> GetLatestMainProcessedMapsetAsync(CancellationToken cancellationToken = default);
    Task<Beatmapset?> GetLatestSecondaryProcessedMapsetAsync(CancellationToken cancellationToken = default);
    Task<Beatmapset?> GetLatestRescannedMapsetAsync(CancellationToken cancellationToken = default);
    Task<Beatmapset?> GetLatestFinishedProcessingMapsetAsync(CancellationToken cancellationToken = default);
    Task<Beatmapset?> GetLatestFinishedRescanningMapsetAsync(CancellationToken cancellationToken = default);
    Task<Beatmapset?> GetLatestBeatmapsetWithNullRankAsync(CancellationToken cancellationToken = default);
}