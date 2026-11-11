namespace Lazerboard.Api.Dtos;

/// <summary>
/// Represent the history of user's score count growth over time
/// </summary>
public class UserHistory
{
    /// <summary>
    /// The historic month
    /// </summary>
    public DateTime Month { get; set; }
    /// <summary>
    /// Accumulated amount of scores up to this month
    /// </summary>
    public int MonthlyCount { get; set; }
}