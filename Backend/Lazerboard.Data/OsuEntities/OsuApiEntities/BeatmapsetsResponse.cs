using Newtonsoft.Json;

namespace Lazerboard.Data.OsuEntities.OsuApiEntities;

public class BeatmapsetsResponse
{
    [JsonProperty("beatmapsets")]
    public List<APIBeatmapset> Beatmapsets { get; set; } = new();
    [JsonProperty("cursor_string")]
    public string Cursor { get; set; } = string.Empty;
}