using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IUserScanLogRepository
{
    Task<UserScanLog?> GetLatestStartedScanAsync(CancellationToken cancellationToken = default);
    Task<UserScanLog?> GetLatestFinishedScanAsync(CancellationToken cancellationToken = default);
}