namespace Lazerboard.Api.Dtos;

/// <summary>
/// Represents an extension of <see cref="ScoresResponse"/> including the overall count of filtered scores
/// </summary>
public class UserScoresResponse : ScoresResponse
{
    /// <summary>
    /// The overall count of filtered scores
    /// </summary>
    public int FilteredCount { get; set; }
}