using osu.Game.Rulesets.Scoring;

namespace Lazerboard.Data.OsuEntities.OsuApiEntities;

public class HitResultAttribute : Attribute
{
    public HitResult HitResult { get; }
        
    public HitResultAttribute(HitResult hitResult)
    {
        HitResult = hitResult;
    }
}