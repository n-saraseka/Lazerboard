using osu.Game.Rulesets.Scoring;

namespace OsuScoreStats.Shared.OsuApi.OsuApiEntities;

public class HitResultAttribute : Attribute
{
    public HitResult HitResult { get; }
        
    public HitResultAttribute(HitResult hitResult)
    {
        HitResult = hitResult;
    }
}