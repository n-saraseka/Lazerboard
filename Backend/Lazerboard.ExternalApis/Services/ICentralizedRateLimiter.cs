namespace Lazerboard.ExternalApis.Services;

public interface ICentralizedRateLimiter
{
    Task WaitForAvailableTokenAsync(bool isHighPriority, CancellationToken ct);
}