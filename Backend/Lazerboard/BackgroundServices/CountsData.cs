using Newtonsoft.Json;

namespace Lazerboard.BackgroundServices;

public class CountsData
{
    [JsonProperty("scoresCount")]
    public int ScoresCount { get; set; }
    [JsonProperty("usersCount")]
    public int UsersCount { get; set; }
    [JsonProperty("registeredUsersCount")]
    public int RegisteredUsersCount { get; set; }
}