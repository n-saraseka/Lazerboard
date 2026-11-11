using Lazerboard.Data.Database.Entities.Enums;

namespace Lazerboard.Data.Database.Entities;

public class UserReport : IEntity<int>
{
    public int Id { get; set; }
    public User? ReportedUser { get; set; }
    public int ReportedUserId { get; set; }
    public User? ReportingUser { get; set; }
    public int ReportingUserId { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.InProgress;
    public DateTimeOffset ReportedOn { get; set; }
}