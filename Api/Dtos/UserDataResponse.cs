namespace OsuScoreStats.Api.Dtos;

public class UserDataResponse
{
    public int Count { get; set; }
    public List<UserHistory> History { get; set; }
}