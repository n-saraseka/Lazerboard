namespace Lazerboard.Api.Dtos;

public class UserRankingResponse
{
    public List<UserRanking> UserRankings { get; set; } = new();
    public int Count { get; set; }
}