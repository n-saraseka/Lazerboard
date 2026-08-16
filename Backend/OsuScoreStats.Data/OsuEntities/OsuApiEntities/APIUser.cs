using Newtonsoft.Json;

namespace OsuScoreStats.Data.OsuEntities.OsuApiEntities;

public class APIUser
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("username")]
    public string Username { get; set; }
    [JsonProperty("country_code")]
    public string CountryCode { get; set; }
    [JsonProperty("country")]
    public APICountry Country { get; set; }
}
