using Lazerboard.Data.OsuEntities.Enums;

namespace Lazerboard.Data.SharedEntities;

public class BeatmapQueryCommand
{
    /// <summary>
    /// The beatmapset ID
    /// </summary>
    public int? BeatmapsetId { get; set; }

    /// <summary>
    /// Array of beatmap IDs
    /// </summary>
    public int[] BeatmapIds { get; set; } = [];

    /// <summary>
    /// Modes to recalculate leaderboards for
    /// </summary>
    public Mode[] Modes { get; set; } = [];
}