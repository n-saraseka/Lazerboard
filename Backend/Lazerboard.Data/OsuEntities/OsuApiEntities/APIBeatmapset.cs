using Newtonsoft.Json;

namespace Lazerboard.Data.OsuEntities.OsuApiEntities;

public class APIBeatmapset
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("artist")]
    public string Artist { get; set; }
    [JsonProperty("title")]
    public string Title { get; set; }
    [JsonProperty("preview_url")]
    public string PreviewUrl { get; set; }
    [JsonProperty("beatmaps")]
    public APIBeatmap[] Beatmaps { get; set; } = Array.Empty<APIBeatmap>();
    [JsonProperty("creator")]
    public string Creator { get; set; }
    [JsonProperty("user_id")]
    public int UserId { get; set; }
    [JsonProperty("ranked_date")]
    public DateTime RankedDate { get; set; }
}