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
    /// The overall count of scores
    /// </summary>
    public int Count { get; set; }
}