using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IUserReportsRepository : IRepository<UserReport, int>
{
    public Task<List<UserReport>> GetRecentReportsDoneByUserAsync(int userId, TimeSpan interval, CancellationToken ct = default);
    public Task<List<UserReport>> GetInProgressReportsAsync(CancellationToken ct = default);
    public Task<UserReport?> GetInProgressUserReportAsync(int userId, CancellationToken ct = default);
}