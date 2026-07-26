using Newtonsoft.Json;
using OsuScoreStats.OsuApi.Enums;

namespace OsuScoreStats.OsuApi.OsuApiEntities;

public class APIBeatmap
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("beatmapset_id")]
    public int BeatmapsetId { get; set; }
    [JsonProperty("beatmapset")]
    public Beatmapset? Beatmapset { get; set; }
    [JsonProperty("mode")]
    public Mode Mode { get; set; }
    [JsonProperty("version")]
    public string DifficultyName { get; set; }
    [JsonProperty("difficulty_rating")]
    public float Difficulty { get; set; }
    [JsonProperty("bpm")]
    public float? BPM {  get; set; }
    [JsonProperty("ar")]
    public float ApproachRate { get; set; }
    [JsonProperty("cs")]
    public float CircleSize { get; set; }
    [JsonProperty("accuracy")]
    public float OverallDifficulty { get; set; }
    [JsonProperty("drain")]
    public float DrainLength { get; set; }
    [JsonProperty("status")]
    public BeatmapStatus Status { get; set; }
}