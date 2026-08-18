using OsuScoreStats.Api.Dtos;
using OsuScoreStats.Data.Database.Entities;

namespace OsuScoreStats.ViewModels;

public class ScoreRankingViewModel
{
    public List<Country> Countries { get; set; } = new();
    public UserRankingResponse? UserRanking { get; set; }
}