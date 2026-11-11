namespace Lazerboard.Api.Dtos;

/// <summary>
/// Represents the speed distribution for this user
/// </summary>
public class UserSpeedStats
{
    /// <summary>
    /// The speed bracket
    /// (in increments of 0.1 for speeds greater or equal to 1, and in increments of 0.05 below)
    /// </summary>
    public double SpeedBracket { get; set; }
    /// <summary>
    /// The overall count of scores within this speed bracket
    /// </summary>
    public int Count { get; set; }
}