using Newtonsoft.Json;

namespace OsuScoreStats.Data.OsuEntities.OsuApiEntities;

public class APICountry
{
    [JsonProperty("code")]
    public string Code { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
}
