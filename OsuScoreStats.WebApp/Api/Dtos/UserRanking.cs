using OsuScoreStats.Shared.DbService.Entities;

namespace OsuScoreStats.Api.Dtos;

public class UserRanking
{
    public int Rank { get; set; }
    public User User { get; set; } = new();
    public int ScoresCount { get; set; }
}