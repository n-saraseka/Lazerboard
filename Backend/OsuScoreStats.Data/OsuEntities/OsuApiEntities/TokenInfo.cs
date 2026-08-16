using Newtonsoft.Json;

namespace OsuScoreStats.Data.OsuEntities.OsuApiEntities;

public class TokenInfo
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
    [JsonProperty("expires_in")]
    public long ExpiresIn { get; set; }
}