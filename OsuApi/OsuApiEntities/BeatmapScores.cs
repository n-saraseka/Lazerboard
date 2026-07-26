using Newtonsoft.Json;
namespace OsuScoreStats.OsuApi.OsuApiEntities;

public class BeatmapScores
{
    [JsonProperty("scores")]
    public Score[] Scores { get; set; } = Array.Empty<Score>();
}