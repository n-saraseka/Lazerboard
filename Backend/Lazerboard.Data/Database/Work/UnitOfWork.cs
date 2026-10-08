using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lazerboard.Data.Database.Work;

public class UnitOfWork: IUnitOfWork
{
    private readonly IServiceScope _scope;
    private readonly ScoreDataContext _dbContext;
    private readonly ILogger<IUnitOfWork> _logger;
    
    public IBeatmapRepository Beatmaps { get; }
    public IBeatmapsetRepository Beatmapsets { get; }
    public IBeatmapsetScanLogRepository BeatmapsetScanLogs { get; }
    public ICountryRepository Countries { get; }
    public IRemovedBeatmapsetRepository RemovedBeatmapsets { get; }
    public IScoreRepository Scores { get; }
    public IUnlistedScoreRepository UnlistedScores { get; }
    public IUserRepository Users { get; }
    public IUserScanLogRepository UserScanLogs { get; }

    public UnitOfWork(IServiceScope scope)
    {
        _scope = scope;
        _dbContext = scope.ServiceProvider.GetRequiredService<ScoreDataContext>();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<IUnitOfWork>>();
        
        Beatmaps = scope.ServiceProvider.GetRequiredService<IBeatmapRepository>();
        Beatmapsets = scope.ServiceProvider.GetRequiredService<IBeatmapsetRepository>();
        BeatmapsetScanLogs = scope.ServiceProvider.GetRequiredService<IBeatmapsetScanLogRepository>();
        Countries = scope.ServiceProvider.GetRequiredService<ICountryRepository>();
        RemovedBeatmapsets = scope.ServiceProvider.GetRequiredService<IRemovedBeatmapsetRepository>();
        Scores = scope.ServiceProvider.GetRequiredService<IScoreRepository>();
        UnlistedScores = scope.ServiceProvider.GetRequiredService<IUnlistedScoreRepository>();
        Users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        UserScanLogs = scope.ServiceProvider.GetRequiredService<IUserScanLogRepository>();
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Database.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, ex, "Transaction commit failed");
            await RollbackTransactionAsync(cancellationToken);
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.RollbackTransactionAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        _scope.Dispose();
        GC.SuppressFinalize(this);
    }
}