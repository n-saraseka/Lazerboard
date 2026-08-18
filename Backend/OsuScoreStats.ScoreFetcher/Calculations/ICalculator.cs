using OsuScoreStats.Data.OsuEntities.OsuApiEntities;

namespace OsuScoreStats.ScoreFetcher.Calculations;

public interface ICalculator
{
    public Task<float?> CalculateAsync(APIScore apiScore, CancellationToken ct = default);
}