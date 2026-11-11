using Newtonsoft.Json;
using osu.Game.Rulesets.Scoring;

namespace Lazerboard.Data.OsuEntities.OsuApiEntities;

public class Statistics
{
    /// <summary>
    /// Count of Miss hit results
    /// </summary>
    [JsonProperty("miss")]
    [HitResult(HitResult.Miss)]
    public int? CountMiss { get; set; }
    /// <summary>
    /// Count of Meh hit results
    /// </summary>
    [JsonProperty("meh")]
    [HitResult(HitResult.Meh)]
    public int? CountMeh { get; set; }
    /// <summary>
    /// Count of Ok hit results
    /// </summary>
    [JsonProperty("ok")]
    [HitResult(HitResult.Ok)]
    public int? CountOk { get; set; }
    /// <summary>
    /// Count of Good hit results
    /// </summary>
    [JsonProperty("good")]
    [HitResult(HitResult.Good)]
    public int? CountGood { get; set; }
    /// <summary>
    /// Count of Great hit results
    /// </summary>
    [JsonProperty("great")]
    [HitResult(HitResult.Great)]
    public int? CountGreat { get; set; }
    /// <summary>
    /// Count of Perfect hit results
    /// </summary>
    [JsonProperty("perfect")]
    [HitResult(HitResult.Perfect)]
    public int? CountPerfect { get; set; }
    /// <summary>
    /// Count of spinner bonuses
    /// </summary>
    [JsonProperty("large_bonus")]
    [HitResult(HitResult.LargeBonus)]
    public int? SpinnerBonus { get; set; }
    /// <summary>
    /// Count of spinner spins
    /// </summary>
    [JsonProperty("small_bonus")]
    [HitResult(HitResult.SmallBonus)]
    public int? SpinnerSpins { get; set; }
    /// <summary>
    /// Count of legacy slider end hits
    /// </summary>
    [JsonProperty("small_tick_hit")]
    [HitResult(HitResult.SmallTickHit)]
    public int? LegacySliderEnds { get; set; }
    /// <summary>
    /// Count of legacy slider end misses
    /// </summary>
    [JsonProperty("small_tick_miss")]
    [HitResult(HitResult.SmallTickMiss)]
    public int? LegacySliderEndMisses { get; set; }
    /// <summary>
    /// Count of slider tick hits
    /// </summary>
    [JsonProperty("large_tick_hit")]
    [HitResult(HitResult.LargeTickHit)]
    public int? SliderTicks { get; set; }
    /// <summary>
    /// Count of slider tick misses
    /// </summary>
    [JsonProperty("large_tick_miss")]
    [HitResult(HitResult.LargeTickMiss)]
    public int? SliderTickMisses { get; set; }
    /// <summary>
    /// Count of slider end hits
    /// </summary>
    [JsonProperty("slider_tail_hit")]
    [HitResult(HitResult.SliderTailHit)]
    public int? SliderEnds { get; set; }
}
