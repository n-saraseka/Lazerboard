using Newtonsoft.Json;

namespace OsuScoreStats.OsuApi.OsuApiEntities;

public class Country
{
    [JsonProperty("code")]
    public string Code { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
}
