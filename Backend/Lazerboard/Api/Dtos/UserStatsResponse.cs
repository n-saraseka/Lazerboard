namespace Lazerboard.Api.Dtos;

public class UserStatsResponse
{
    public List<UserHistory> History { get; set; } = [];
    public List<RankStats> RankDistribution { get; set; } = [];
    public List<UserStars> StarsDistribution { get; set; } = [];
    public List<UserSpeedStats> SpeedDistribution { get; set; } = [];
}