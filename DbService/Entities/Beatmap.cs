using OsuScoreStats.OsuApi.Enums;

namespace OsuScoreStats.DbService.Entities;

public class Beatmap : IEntity<int>
{
    public int Id { get; set; }
    public int BeatmapsetId { get; set; }
    public Beatmapset Beatmapset { get; set; }
    public Mode Mode { get; set; }
    public string DifficultyName { get; set; }
    public float Difficulty { get; set; }
    public float? BPM {  get; set; }
    public float ApproachRate { get; set; }
    public float CircleSize { get; set; }
    public float OverallDifficulty { get; set; }
    public float DrainLength { get; set; }
    public BeatmapStatus Status { get; set; }
    public List<Score> Scores { get; set; }
}