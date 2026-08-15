namespace OsuScoreStats.OsuApi;

public interface ICentralizedRateLimiter
{
    Task WaitForAvailableTokenAsync(CancellationToken ct);
}