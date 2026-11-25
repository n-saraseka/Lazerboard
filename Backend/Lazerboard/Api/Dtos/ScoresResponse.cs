namespace Lazerboard.Api.Dtos;

/// <summary>
/// Represents an object that returns some scores and the overall count of scores (for pagination)
/// </summary>
public class ScoresResponse
{
    /// <summary>
    /// A list of scores
    /// </summary>
    public List<ApiScore> Scores { get; set; } = new();
    /// <summary>
    /// Whether there are more pages available or not
    /// </summary>
    public bool MoreAvailable { get; set; }
}