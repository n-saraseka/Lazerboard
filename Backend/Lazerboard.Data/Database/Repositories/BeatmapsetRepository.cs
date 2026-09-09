using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Data.Database.Repositories;

public class BeatmapsetRepository(ScoreDataContext db) : BaseRepository<Beatmapset, int>(db), IBeatmapsetRepository
{
    /// <summary>
    /// Get the <see cref="Beatmapset"/> with the largest <see cref="Beatmapset.MainStartedProcessingAt"/> value
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="Beatmapset"/> or null</returns>
    public Task<Beatmapset?> GetLatestMainProcessedMapsetAsync(CancellationToken cancellationToken = default) =>
        Set
            .Where(bs => bs.MainStartedProcessingAt != null)
            .OrderByDescending(bs => bs.MainStartedProcessingAt)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Get the <see cref="Beatmapset"/> with the largest <see cref="Beatmapset.SecondaryStartedProcessingAt"/> value
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="Beatmapset"/> or null</returns>
    public Task<Beatmapset?> GetLatestSecondaryProcessedMapsetAsync(CancellationToken cancellationToken = default) =>
        Set
            .Where(bs => bs.SecondaryStartedProcessingAt != null)
            .OrderByDescending(bs => bs.SecondaryStartedProcessingAt)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Get the <see cref="Beatmapset"/> with the largest <see cref="Beatmapset.StartedScanningAt"/> value
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="Beatmapset"/> or null</returns>
    public Task<Beatmapset?> GetLatestRescannedMapsetAsync(CancellationToken cancellationToken = default) =>
        Set
            .Where(bs => bs.StartedScanningAt != null)
            .OrderByDescending(bs => bs.StartedScanningAt)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Get the <see cref="Beatmapset"/> with the largest <see cref="Beatmapset.MainFinishedProcessingAt"/> value
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="Beatmapset"/> or null</returns>
    public Task<Beatmapset?> GetLatestFinishedProcessingMapsetAsync(CancellationToken cancellationToken = default) =>
        Set
            .Where(bs => bs.MainFinishedProcessingAt != null || bs.SecondaryFinishedProcessingAt != null)
            .OrderByDescending(bs => bs.MainFinishedProcessingAt)
            .FirstOrDefaultAsync(cancellationToken);
    
    /// <summary>
    /// Get the <see cref="Beatmapset"/> with the largest <see cref="Beatmapset.FinishedScanningAt"/> value
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="Beatmapset"/> or null</returns>
    public Task<Beatmapset?> GetLatestFinishedRescanningMapsetAsync(CancellationToken cancellationToken = default) =>
        Set
            .Where(bs => bs.FinishedScanningAt != null)
            .OrderByDescending(bs => bs.FinishedScanningAt)
            .FirstOrDefaultAsync(cancellationToken);
    
    /// <summary>
    /// Get the <see cref="Beatmapset"/> with the largest <see cref="Beatmapset.Id"/>
    /// where <see cref="Beatmapset.RankedDate"/> is null
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="Beatmapset"/> or null</returns>
    public Task<Beatmapset?> GetLatestBeatmapsetWithNullRankAsync(CancellationToken cancellationToken = default) =>
        Set
            .Where(bs => bs.RankedDate == null)
            .OrderByDescending(bs => bs.Id)
            .FirstOrDefaultAsync(cancellationToken);
}
