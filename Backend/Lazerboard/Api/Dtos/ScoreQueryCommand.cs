using Lazerboard.Data.OsuEntities.Enums;

namespace Lazerboard.Api.Dtos;

/// <summary>
/// A helper class for building score queries. The range parameters take the minimum first and the maximum second.
/// </summary>
public class ScoreQueryCommand
{
    /// <summary>
    /// An array with allowed modes
    /// </summary>
    public Mode[]? Modes { get; set; }

    /// <summary>
    /// A list with two date values
    /// </summary>
    public List<DateOnly?> DateRange { get; set; } = [null, null];
    
    /// <summary>
    /// A list with two rank values
    /// </summary>
    public List<int?> RankRange { get; set; } = [null, null];
    
    /// <summary>
    /// A list with two PP values
    /// </summary>
    public List<float?> PpRange { get; set; } = [null, null];
    
    /// <summary>
    /// A list with two accuracy values
    /// </summary>
    public List<float?> AccuracyRange { get; set; } = [null, null];
    
    /// <summary>
    /// A list with two speed values
    /// </summary>
    public List<double?> SpeedRange { get; set; } = [null, null];
    
    /// <summary>
    /// A list with two star rating values
    /// </summary>
    public List<float?> StarRange { get; set; } = [null, null];
    
    /// <summary>
    /// An array of mod acronyms to include in scores
    /// </summary>
    public string[] IncludeMods { get; set; } = [];
    
    /// <summary>
    /// An array of mod acronyms to exclude from scores
    /// </summary>
    public string[] ExcludeMods { get; set; } = [];
    
    /// <summary>
    /// Whether to allow other mods than those listed in <see cref="IncludeMods"/> or not
    /// </summary>
    public bool? LenientMode { get; set; }
    
    /// <summary>
    /// The country code to filter scores by
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Name of the field to sort scores by
    /// </summary>
    public string? SortBy { get; set; } = "PP";
    
    /// <summary>
    /// Whether sort is descending or not
    /// </summary>
    public bool? IsDescending { get; set; }
    
    /// <summary>
    /// Whether to exclude scores that have the convert flag set to true or not
    /// </summary>
    public bool? ExcludeConverts { get; set; }
    
    /// <summary>
    /// The beatmap status to filter for. Set to null if you don't want to filter by beatmap status.
    /// </summary>
    public BeatmapStatus? BeatmapStatus { get; set; }
}