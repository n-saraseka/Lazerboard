using OsuScoreStats.Shared.DbService.Entities;
using OsuScoreStats.Shared.OsuApi.Enums;

namespace OsuScoreStats.ViewModels;

public class BeatmapsetViewModel
{
    public Beatmapset Beatmapset { get; set; }
    public List<Beatmap> Beatmaps { get; set; }
    public int SelectedBeatmapId { get; set; }
    public List<Score> Scores { get; set; }
    public Mode SelectedMode { get; set; }
    public int Pages { get; set; }
}