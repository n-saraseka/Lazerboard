namespace Lazerboard.Data.Database.Entities;

public class RemovedBeatmapset : IEntity<int>
{
    public int Id { get; set; }
    public string Artist { get; set; }
    public string Title { get; set; }
    public string Creator { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public DateTimeOffset? RankedDate { get; set; }
    public DateTimeOffset? ArchivedDate { get; set; }
}