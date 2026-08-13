using OsuScoreStats.DbService.Entities;

namespace OsuScoreStats.Api.Dtos;

public class ScoresResponse
{
    public List<Score> Scores { get; set; } = new();
    public int Count { get; set; }
}