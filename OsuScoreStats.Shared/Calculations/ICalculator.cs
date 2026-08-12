using OsuScoreStats.Shared.OsuApi.OsuApiEntities;

namespace OsuScoreStats.Shared.Calculations;

public interface ICalculator
{
    public Task<float?> CalculateAsync(APIScore apiScore, CancellationToken ct = default);
}