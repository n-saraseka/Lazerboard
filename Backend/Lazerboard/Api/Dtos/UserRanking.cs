using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Api.Dtos;

public class UserRanking
{
    public int Rank { get; set; }
    public User User { get; set; } = new();
    public int ScoresCount { get; set; }
}