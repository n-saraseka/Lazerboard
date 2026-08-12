using OsuScoreStats.Api.Dtos;
using OsuScoreStats.Shared.DbService.Entities;

namespace OsuScoreStats.ViewModels;

public class ScoreRankingViewModel
{
    public List<Country> Countries { get; set; } = new();
    public UserRankingResponse? UserRanking { get; set; }
}