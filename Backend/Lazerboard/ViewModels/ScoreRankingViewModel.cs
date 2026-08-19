using Lazerboard.Api.Dtos;
using Lazerboard.Data.Database.Entities;

namespace Lazerboard.ViewModels;

public class ScoreRankingViewModel
{
    public List<Country> Countries { get; set; } = new();
    public UserRankingResponse? UserRanking { get; set; }
}