using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Data.Database.Repositories.Interfaces;

public interface IUserReportsRepository : IRepository<UserReport, int>
{
    public Task<List<UserReport>> GetRecentReportsDoneByUser(int userId, TimeSpan interval, CancellationToken ct = default);
    public Task<List<UserReport>> GetInProgressReports(CancellationToken ct = default);
    public Task<UserReport?> GetInProgressUserReport(int userId, CancellationToken ct = default);
}