using Newtonsoft.Json;

namespace OsuScoreStats.Shared.OsuApi.OsuApiEntities;

public class ScoresResponse
{
    [JsonProperty("scores")]
    public APIScore[] Scores { get; set; }
    [JsonProperty("cursor_string")]
    public string Cursor { get; set; }
}
