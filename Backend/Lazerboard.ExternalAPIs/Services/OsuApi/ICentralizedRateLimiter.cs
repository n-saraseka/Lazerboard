namespace Lazerboard.ExternalAPIs.Services.OsuApi;

public interface ICentralizedRateLimiter
{
    Task WaitForAvailableTokenAsync(CancellationToken ct);
}