using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Api.Dtos;

/// <summary>
/// Represents an object containing all <see cref="User"/> <see cref="Score"/> statistics
/// </summary>
public class UserStatsResponse
{
    /// <summary>
    /// A list with <see cref="User"/>'s <see cref="UserHistory"/>
    /// </summary>
    public List<UserHistory> History { get; set; } = [];
    /// <summary>
    /// A list with <see cref="User"/>'s <see cref="RankStats"/>
    /// </summary>
    public List<RankStats> RankDistribution { get; set; } = [];
    /// <summary>
    /// A list with <see cref="User"/>'s <see cref="UserStars"/>
    /// </summary>
    public List<UserStars> StarsDistribution { get; set; } = [];
    /// <summary>
    /// A list with <see cref="User"/>'s <see cref="UserSpeedStats"/>
    /// </summary>
    public List<UserSpeedStats> SpeedDistribution { get; set; } = [];
}