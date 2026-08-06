using OsuScoreStats.DbService.Entities;

namespace OsuScoreStats.ViewModels;

public class ScoreRankingViewModel
{
    public List<Country> Countries { get; set; } = new();
}