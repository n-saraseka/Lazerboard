using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.Calculators;

public interface ICalculator
{
    public Task<float> CalculateAsync(APIScore apiScore, CancellationToken ct = default);
}