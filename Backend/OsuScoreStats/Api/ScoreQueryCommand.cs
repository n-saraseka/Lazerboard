using OsuScoreStats.DbService.Entities;
using OsuScoreStats.OsuApi.Enums;

namespace OsuScoreStats.Api;

/// <summary>
/// A helper class for building score queries.
/// </summary>
/// <remarks>The {x}Range parameters refer to <see cref="List{T}"/>s of two values where the first one is the minimum value and the second one is the maximum value.
/// When provided, it filters the query to look up scores between the two values.</remarks>
public class ScoreQueryCommand
{
    /// <summary>
    /// An array with allowed <see cref="Score.Mode"/>s
    /// </summary>
    public Mode[] Modes { get; set; } = [];

    /// <summary>
    /// A <see cref="List{DateOnly}"/> with two <see cref="Score.Date"/> values
    /// </summary>
    public List<DateOnly?> DateRange { get; set; } = new();
    
    /// <summary>
    /// A <see cref="List{int}"/> with two <see cref="Score.Rank"/> values
    /// </summary>
    public List<int?> RankRange { get; set; } = new();
    
    /// <summary>
    /// A <see cref="List{int}"/> with two <see cref="Score.PP"/> values
    /// </summary>
    public List<int?> PpRange { get; set; } = new();
    
    /// <summary>
    /// A <see cref="List{double}"/> with two <see cref="Score.Accuracy"/> values
    /// </summary>
    public List<double?> AccuracyRange { get; set; } = new();
    
    /// <summary>
    /// A <see cref="List{double}"/> with two <see cref="Score.SpeedChange"/> values
    /// </summary>
    public List<double?> SpeedRange { get; set; } = new();
    
    /// <summary>
    /// A <see cref="List{double}"/> with two <see cref="Beatmap.Difficulty"/> values
    /// </summary>
    public List<double?> StarRange { get; set; } = new();
    
    /// <summary>
    /// An array of mod acronyms to include in <see cref="Score"/>s
    /// </summary>
    public string[] IncludeMods { get; set; } = [];
    
    /// <summary>
    /// An array of mod acronyms to exclude from <see cref="Score"/>s
    /// </summary>
    public string[] ExcludeMods { get; set; } = [];
    
    /// <summary>
    /// Whether to allow other mods than those listed in <see cref="IncludeMods"/> or not
    /// </summary>
    public bool? LenientMode { get; set; }
    
    /// <summary>
    /// The <see cref="Country"/> code to filter <see cref="Score"/>s by
    /// </summary>
    public string? CountryCode { get; set; }
    
    /// <summary>
    /// Name of the field to sort <see cref="Score"/>s by
    /// </summary>
    public string? SortBy { get; set; }
    
    /// <summary>
    /// Whether sort is descending or not
    /// </summary>
    public bool? IsDescending { get; set; }
}