using Newtonsoft.Json;

namespace OsuScoreStats.Data.OsuEntities.OsuApiEntities;

public class BeatmapScores
{
    [JsonProperty("scores")]
    public APIScore[] Scores { get; set; } = Array.Empty<APIScore>();
}