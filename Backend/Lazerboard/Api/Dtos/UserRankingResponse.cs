namespace Lazerboard.Api.Dtos;

/// <summary>
/// Represents an object that returns some user rankings
/// and the overall count of users within the ranking (for pagination)
/// </summary>
public class UserRankingResponse
{
    /// <summary>
    /// A list of user rankings
    /// </summary>
    public List<UserRanking> UserRankings { get; set; } = new();
    /// <summary>
    /// Overall count of users within the ranking
    /// </summary>
    public int Count { get; set; }
}