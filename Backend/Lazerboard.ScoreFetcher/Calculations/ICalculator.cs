using Lazerboard.Data.OsuEntities.OsuApiEntities;
using osu.Game.Beatmaps;

namespace Lazerboard.ScoreFetcher.Calculations;

public interface ICalculator
{
    public Task<float?> CalculateAsync(APIScore apiScore, FlatWorkingBeatmap flatWorkingBeatmap, CancellationToken ct = default);
}