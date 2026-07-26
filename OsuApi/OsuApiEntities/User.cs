using Newtonsoft.Json;

namespace OsuScoreStats.OsuApi.OsuApiEntities;

public class User
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("username")]
    public string Username { get; set; }
    [JsonProperty("country_code")]
    public string CountryCode { get; set; }
    [JsonProperty("country")]
    public Country Country { get; set; }
}
