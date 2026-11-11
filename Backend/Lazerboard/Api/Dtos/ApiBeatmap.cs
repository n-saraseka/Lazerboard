using Lazerboard.Data.OsuEntities.Enums;

namespace Lazerboard.Api.Dtos;

/// <summary>
/// Represents a beatmap
/// </summary>
public class ApiBeatmap
{
    /// <summary>
    /// The beatmap ID
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// The beatmapset ID
    /// </summary>
    public int BeatmapsetId { get; set; }
    /// <summary>
    /// The beatmapset
    /// </summary>
    public ApiBeatmapset? Beatmapset { get; set; }
    /// <summary>
    /// The base gameplay mode
    /// </summary>
    public Mode Mode { get; set; }
    /// <summary>
    /// The difficulty name
    /// </summary>
    public string DifficultyName { get; set; }
    /// <summary>
    /// The star rating
    /// </summary>
    public float Difficulty { get; set; }
    /// <summary>
    /// The BPM
    /// </summary>
    public float? BPM { get; set; }
    /// <summary>
    /// The approach rate (AR)
    /// </summary>
    public float ApproachRate { get; set; }
    /// <summary>
    /// The circle size (CS)
    /// </summary>
    public float CircleSize { get; set; }
    /// <summary>
    /// The overall difficulty (OD)
    /// </summary>
    public float OverallDifficulty { get; set; }
    /// <summary>
    /// The health drain (HP)
    /// </summary>
    public float? Health { get; set; }
    /// <summary>
    /// The length of a beatmap in seconds
    /// </summary>
    public int DrainLength { get; set; }
    /// <summary>
    /// The <see cref="BeatmapStatus"/>
    /// </summary>
    public BeatmapStatus Status { get; set; }
}