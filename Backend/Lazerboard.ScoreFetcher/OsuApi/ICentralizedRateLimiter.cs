namespace Lazerboard.ScoreFetcher.OsuApi;

public interface ICentralizedRateLimiter
{
    Task WaitForAvailableTokenAsync(CancellationToken ct);
}