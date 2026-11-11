namespace Lazerboard.Api.Dtos;

/// <summary>
/// Represents a ranking of users based on scores count
/// </summary>
public class UserRanking
{
    /// <summary>
    /// The user's rank
    /// </summary>
    public int Rank { get; set; }
    /// <summary>
    /// The user
    /// </summary>
    public ApiUser User { get; set; } = new();
    /// <summary>
    /// The scores count for this user
    /// </summary>
    public int ScoresCount { get; set; }
}