using Lazerboard.Data.Database.Repositories.Interfaces;

namespace Lazerboard.Data.Database.Work;

public interface IUnitOfWork : IAsyncDisposable
{
    IBeatmapRepository Beatmaps { get; }
    IBeatmapsetRepository Beatmapsets { get; }
    IBeatmapsetScanLogRepository BeatmapsetScanLogs { get; }
    ICountryRepository Countries { get; }
    IRemovedBeatmapsetRepository RemovedBeatmapsets { get; }
    IScoreRepository Scores { get; }
    IUnlistedScoreRepository UnlistedScores { get; }
    IUserRepository Users { get; }
    IUserScanLogRepository UserScanLogs { get; }
    
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}