using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.Calculators;

public interface ICalculator
{
    public Task<float> CalculateAsync(Score score, CancellationToken ct = default);
}