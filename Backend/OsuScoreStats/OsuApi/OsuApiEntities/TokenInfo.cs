using Newtonsoft.Json;

namespace OsuScoreStats.OsuApi.OsuApiEntities;

public class TokenInfo
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
    [JsonProperty("expires_in")]
    public long ExpiresIn { get; set; }
}