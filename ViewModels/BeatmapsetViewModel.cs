using OsuScoreStats.DbService.Entities;

namespace OsuScoreStats.ViewModels;

public class BeatmapsetViewModel
{
    public Beatmapset Beatmapset { get; set; }
    public List<Beatmap> Beatmaps { get; set; }
    public int SelectedBeatmapId { get; set; }
}