using Newtonsoft.Json;

namespace OsuScoreStats.OsuApi.OsuApiEntities;

public class BeatmapScores
{
    [JsonProperty("scores")]
    public APIScore[] Scores { get; set; } = Array.Empty<APIScore>();
}