namespace Lazerboard.ExternalApis.Services;

public interface ICentralizedRateLimiter
{
    Task WaitForAvailableTokenAsync(CancellationToken ct);
}