using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IUserScanLogRepository
{
    Task<UserScanLog?> GetLatestStartedScanAsync(CancellationToken cancellationToken = default);
    Task<UserScanLog?> GetLatestFinishedScanAsync(CancellationToken cancellationToken = default);
    Task<UserScanLog?> GetLatestStartedCheckAsync(CancellationToken cancellationToken = default);
    Task<UserScanLog?> GetLatestFinishedCheckAsync(CancellationToken cancellationToken = default);
    Task<int> SaveEventAsync(ScanEventType type, DateTime dateTime, CancellationToken cancellationToken = default);
}