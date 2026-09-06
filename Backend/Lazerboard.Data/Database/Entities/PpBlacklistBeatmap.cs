using Lazerboard.Data.Database.Entities.Enums;

namespace Lazerboard.Data.Database.Entities;

public class PpBlacklistBeatmap : IEntity<int>
{
    public int Id { get; set; }
    public Beatmap Beatmap { get; set; }
    public int BeatmapId { get; set; }
    public DateTime DateAdded { get; set; }
    public BlacklistReason Reason { get; set; }
}