using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Game.Online.API;
using OsuScoreStats.Shared.OsuApi.Enums;

namespace OsuScoreStats.Shared.OsuApi.OsuApiEntities;

public class APIScore
{
    [JsonProperty("id")]
    public ulong Id { get; set; }
    [JsonProperty("legacy_score_id")]
    public ulong? LegacyScoreId { get; set; }
    [JsonProperty("ended_at")]
    public DateTime Date {  get; set; }
    [JsonProperty("ruleset_id")]
    public Mode Mode { get; set; }
    [JsonProperty("beatmap_id")]
    public int BeatmapId { get; set; }
    [JsonProperty("user")]
    public APIUser User { get; set; } = new();
    [JsonProperty("user_id")]
    public int UserId { get; set; }
    [JsonProperty("rank")]
    [JsonConverter(typeof(StringEnumConverter))]
    public Grade Grade { get; set; }
    [JsonProperty("mods")]
    public APIMod[] Mods { get; set; } = Array.Empty<APIMod>();
    [JsonProperty("accuracy")]
    public float Accuracy { get; set; }
    [JsonProperty("max_combo")]
    public int Combo { get; set; }
    [JsonProperty("statistics")]
    public Statistics Statistics { get; set; } = null!;
    [JsonProperty("maximum_statistics")]
    public Statistics MaximumStatistics { get; set; } = null!;
    [JsonProperty("total_score")]
    public long TotalScore { get; set; }
    [JsonProperty("classic_total_score")]
    public int ClassicTotalScore { get; set; }
    [JsonProperty("legacy_total_score")]
    public int LegacyTotalScore { get; set; }
    [JsonProperty("pp")]
    public float? PP { get; set; }
}
