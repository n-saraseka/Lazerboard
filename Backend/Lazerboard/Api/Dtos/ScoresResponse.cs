using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Api.Dtos;

public class ScoresResponse
{
    public List<Score> Scores { get; set; } = new();
    public int Count { get; set; }
}