using Newtonsoft.Json;

namespace OsuScoreStats.Data.OsuEntities.OsuApiEntities;

public class ScoresResponse
{
    [JsonProperty("scores")]
    public APIScore[] Scores { get; set; }
    [JsonProperty("cursor_string")]
    public string Cursor { get; set; }
}
