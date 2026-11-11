using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;

namespace Lazerboard.Api.Dtos;

/// <summary>
/// Represents a score
/// </summary>
public class ApiScore
{
    /// <summary>
    /// The score ID
    /// </summary>
    public ulong Id { get; set; }
    /// <summary>
    /// The date when the score was set
    /// </summary>
    public DateTime Date {  get; set; }
    /// <summary>
    /// The gameplay mode
    /// </summary>
    public Mode Mode { get; set; }
    /// <summary>
    /// The beatmap
    /// </summary>
    public ApiBeatmap? Beatmap { get; set; }
    /// <summary>
    /// The beatmap ID
    /// </summary>
    public int BeatmapId { get; set; }
    /// <summary>
    /// The user
    /// </summary>
    public ApiUser? User { get; set; }
    /// <summary>
    /// The user ID
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// The grade 
    /// </summary>
    public Grade Grade { get; set; }
    /// <summary>
    /// A list with acronyms of mods used when setting the score
    /// </summary>
    public List<string> ModAcronyms { get; set; } = new();
    /// <summary>
    /// The speed the map was played at
    /// </summary>
    public double? SpeedChange { get; set; }
    /// <summary>
    /// The accuracy
    /// </summary>
    public float Accuracy { get; set; }
    /// <summary>
    /// The maximum combo
    /// </summary>
    public int Combo { get; set; }
    /// <summary>
    /// The total score (max 1 million without mods)
    /// </summary>
    public int TotalScore { get; set; }
    /// <summary>
    /// The classic total score (an approximation of <see cref="TotalScore"/> converted to scoreV1)
    /// </summary>
    public long ClassicTotalScore { get; set; }
    /// <summary>
    /// The total score achieved using scoreV1 (set to 0 for osu!lazer scores)
    /// </summary>
    public int? LegacyTotalScore { get; set; }
    /// <summary>
    /// The PP value
    /// </summary>
    public float? PP { get; set; }
    /// <summary>
    /// The rank within the map's leaderboard
    /// </summary>
    public int Rank { get; set; }
    /// <summary>
    /// Whether the score was set on a converted map or not
    /// </summary>
    public bool? IsConvert { get; set; }
    /// <summary>
    /// Whether the score was set on osu!lazer or not
    /// </summary>
    public bool? IsLazerScore { get; set; }
    /// <summary>
    /// Whether the score has the maximum combo (in osu!lazer logic) or not
    /// </summary>
    public bool? IsPerfectCombo { get; set; }
    /// <summary>
    /// The <see cref="Statistics"/>
    /// </summary>
    public Statistics? Statistics { get; set; }
}