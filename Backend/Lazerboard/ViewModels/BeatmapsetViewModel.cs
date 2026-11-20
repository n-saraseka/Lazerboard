using Lazerboard.Api.Dtos;
using Lazerboard.Data.OsuEntities.Enums;

namespace Lazerboard.ViewModels;

public class BeatmapsetViewModel
{
    public ApiBeatmapset Beatmapset { get; set; }
    public List<ApiBeatmap> Beatmaps { get; set; }
    public int SelectedBeatmapId { get; set; }
    public Mode SelectedMode { get; set; }
}