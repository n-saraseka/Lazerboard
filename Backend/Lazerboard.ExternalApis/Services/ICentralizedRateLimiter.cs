namespace Lazerboard.ExternalApis.Services;

public interface ICentralizedRateLimiter
{
    Task WaitForAvailableTokenAsync(decimal priority, CancellationToken ct);
}