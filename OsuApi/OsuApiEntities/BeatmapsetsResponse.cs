using Newtonsoft.Json;

namespace OsuScoreStats.OsuApi.OsuApiEntities;

public class BeatmapsetsResponse
{
    [JsonProperty("beatmapsets")]
    public List<Beatmapset> Beatmapsets { get; set; } = new();
    [JsonProperty("cursor_string")]
    public string Cursor { get; set; } = string.Empty;
}