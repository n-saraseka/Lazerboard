namespace Lazerboard.ExternalApis.Services.OsuApi;

public interface ICentralizedRateLimiter
{
    Task WaitForAvailableTokenAsync(CancellationToken ct);
}