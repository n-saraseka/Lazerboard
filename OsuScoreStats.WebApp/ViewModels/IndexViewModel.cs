using OsuScoreStats.Shared.DbService.Entities;

namespace OsuScoreStats.ViewModels;

public class IndexViewModel
{
    public List<Score> Scores { get; set; } = new();
    public int Pages { get; set; }
    public List<Country> Countries { get; set; } = new();
}