using OsuScoreStats.DbService.Entities;

namespace OsuScoreStats.Api.Dtos;

public class UserRanking
{
    public User User { get; set; } = new();
    public int ScoresCount { get; set; }
}