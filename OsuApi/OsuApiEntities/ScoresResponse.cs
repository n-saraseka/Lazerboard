using Newtonsoft.Json;

namespace OsuScoreStats.OsuApi.OsuApiEntities;

public class ScoresResponse
{
    [JsonProperty("scores")]
    public Score[] Scores { get; set; }
    [JsonProperty("cursor_string")]
    public string Cursor { get; set; }
}
