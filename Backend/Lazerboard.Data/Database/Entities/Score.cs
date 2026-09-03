using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.OsuEntities.Enums;

namespace Lazerboard.Data.Database.Entities;

public class Score : IEntity<ulong>
{
    public ulong Id { get; set; }
    public DateTime Date {  get; set; }
    public Mode Mode { get; set; }
    public Beatmap Beatmap { get; set; }
    public int BeatmapId { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
    public Grade Grade { get; set; }
    public List<string> ModAcronyms { get; set; } = new();
    public double? SpeedChange { get; set; }
    public float Accuracy { get; set; }
    public int Combo { get; set; }
    public int? Misses { get; set; }
    public int TotalScore { get; set; }
    public long ClassicTotalScore { get; set; }
    public int? LegacyTotalScore { get; set; }
    public float? PP { get; set; }
    public int Rank { get; set; }
    public ScoreSource ScoreSource { get; set; } = ScoreSource.ScoreFetcher;
}