using OsuScoreStats.OsuApi.Enums;

namespace OsuScoreStats.DbService.Entities;

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
    public long TotalScore { get; set; }
    public int ClassicTotalScore { get; set; }
    public int? LegacyTotalScore { get; set; }
    public float? PP { get; set; }
    public int Rank { get; set; }
}