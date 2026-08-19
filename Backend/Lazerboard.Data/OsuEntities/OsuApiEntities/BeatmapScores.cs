using Newtonsoft.Json;

namespace Lazerboard.Data.OsuEntities.OsuApiEntities;

public class BeatmapScores
{
    [JsonProperty("scores")]
    public APIScore[] Scores { get; set; } = Array.Empty<APIScore>();
}