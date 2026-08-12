using Newtonsoft.Json;

namespace OsuScoreStats.Shared.OsuApi.OsuApiEntities;

public class BeatmapScores
{
    [JsonProperty("scores")]
    public APIScore[] Scores { get; set; } = Array.Empty<APIScore>();
}