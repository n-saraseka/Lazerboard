using Lazerboard.Data.Database.Entities.Enums;

namespace Lazerboard.Data.Database.Entities;

public class BeatmapsetScanLog : IEntity<int>
{
    public int Id { get; set; }
    public DateTime LoggedAt { get; set; }
    public ScanEventType EventType { get; set; }
}