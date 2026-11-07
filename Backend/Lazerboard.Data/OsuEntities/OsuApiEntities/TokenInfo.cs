using Newtonsoft.Json;

namespace Lazerboard.Data.OsuEntities.OsuApiEntities;

public class TokenInfo
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    [JsonProperty("refresh_token")] 
    public string RefreshToken { get; set; } = string.Empty;
    [JsonProperty("expires_in")]
    public long ExpiresIn { get; set; }
}