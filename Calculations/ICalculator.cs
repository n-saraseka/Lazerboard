using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.Calculations;

public interface ICalculator
{
    public Task<float> CalculateAsync(APIScore apiScore, CancellationToken ct = default);
}