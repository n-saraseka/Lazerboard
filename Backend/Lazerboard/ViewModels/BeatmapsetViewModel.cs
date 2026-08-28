using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.OsuEntities.Enums;

namespace Lazerboard.ViewModels;

public class BeatmapsetViewModel
{
    public Beatmapset Beatmapset { get; set; }
    public List<Beatmap> Beatmaps { get; set; }
    public int SelectedBeatmapId { get; set; }
    public Mode SelectedMode { get; set; }
}