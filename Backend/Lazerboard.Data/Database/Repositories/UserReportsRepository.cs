using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lazerboard.Data.Database.Repositories;

public class UserReportsRepository(ScoreDataContext db) : BaseRepository<UserReport, int>(db), IUserReportsRepository
{
    /// <summary>
    /// Get recent reports done by this <see cref="UserReport.ReportingUser"/>
    /// </summary>
    /// <param name="userId">The <see cref="UserReport.ReportingUserId"/></param>
    /// <param name="interval">A <see cref="TimeSpan"/> representing the recent reports window</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>A list of <see cref="UserReport"/>s</returns>
    public Task<List<UserReport>> GetRecentReportsDoneByUserAsync(int userId, TimeSpan interval,
        CancellationToken ct = default)
    {
        var dateTimeMin = DateTimeOffset.UtcNow - interval;
        return Set
            .AsNoTracking()
            .Where(r => r.ReportingUserId == userId && r.ReportedOn >= dateTimeMin)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Get recent reports
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>A list of <see cref="UserReport"/>s</returns>
    public Task<List<UserReport>> GetInProgressReportsAsync(CancellationToken ct = default) => Set
        .AsNoTracking()
        .Where(r => r.Status == ReportStatus.InProgress)
        .Include(r => r.ReportedUser)
        .ToListAsync(ct);
    
    /// <summary>
    /// Get latest report for this <see cref="UserReport.ReportedUser"/>
    /// </summary>
    /// <param name="userId">The <see cref="UserReport.ReportedUserId"/></param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="UserReport"/> or null</returns>
    public Task<UserReport?> GetInProgressUserReportAsync(int userId, CancellationToken ct = default) =>
        Set
            .AsNoTracking()
            .Where(r => r.ReportedUserId == userId && r.Status == ReportStatus.InProgress)
            .OrderByDescending(r => r.ReportedOn)
            .FirstOrDefaultAsync(ct);
}