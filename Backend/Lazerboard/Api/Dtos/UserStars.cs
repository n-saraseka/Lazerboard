namespace Lazerboard.Api.Dtos;

/// <summary>
/// Represents the star rating distribution for this user
/// </summary>
public class UserStars
{
    /// <summary>
    /// The star rating  bracket (in increments of 1)
    /// </summary>
    public double SrBracket { get; set; }
    /// <summary>
    /// The overall count of scores within this star rating  bracket
    /// </summary>
    public int Count { get; set; }
}