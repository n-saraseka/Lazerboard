using Lazerboard.Data.OsuEntities.OsuApiEntities;

namespace Lazerboard.ScoreFetcher.Calculations;

public interface ICalculator
{
    public Task<float?> CalculateAsync(APIScore apiScore, CancellationToken ct = default);
}