using Newtonsoft.Json;
using osu.Game.Rulesets.Scoring;

namespace OsuScoreStats.Data.OsuEntities.OsuApiEntities;

public class Statistics
{
    [JsonProperty("miss")]
    [HitResult(HitResult.Miss)]
    public int? CountMiss { get; set; }
    [JsonProperty("meh")]
    [HitResult(HitResult.Meh)]
    public int? CountMeh { get; set; }
    [JsonProperty("ok")]
    [HitResult(HitResult.Ok)]
    public int? CountOk { get; set; }
    [JsonProperty("good")]
    [HitResult(HitResult.Good)]
    public int? CountGood { get; set; }
    [JsonProperty("great")]
    [HitResult(HitResult.Great)]
    public int? CountGreat { get; set; }
    [JsonProperty("perfect")]
    [HitResult(HitResult.Perfect)]
    public int? CountPerfect { get; set; }
    [JsonProperty("large_bonus")]
    [HitResult(HitResult.LargeBonus)]
    public int? SpinnerBonus { get; set; }
    [JsonProperty("small_bonus")]
    [HitResult(HitResult.SmallBonus)]
    public int? SpinnerSpins { get; set; }
    [JsonProperty("small_tick_hit")]
    [HitResult(HitResult.SmallTickHit)]
    public int? LegacySliderEnds { get; set; }
    [JsonProperty("small_tick_miss")]
    [HitResult(HitResult.SmallTickMiss)]
    public int? LegacySliderEndMisses { get; set; }
    [JsonProperty("large_tick_hit")]
    [HitResult(HitResult.LargeTickHit)]
    public int? SliderTicks { get; set; }
    [JsonProperty("large_tick_miss")]
    [HitResult(HitResult.LargeTickMiss)]
    public int? SliderTickMisses { get; set; }
    [JsonProperty("slider_tail_hit")]
    [HitResult(HitResult.SliderTailHit)]
    public int? SliderEnds { get; set; }
}
