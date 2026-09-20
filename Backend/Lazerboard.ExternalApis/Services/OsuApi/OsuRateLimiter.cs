namespace Lazerboard.ExternalApis.Services.OsuApi;


public class OsuRateLimiter(OsuApiQueue queue) : ICentralizedRateLimiter
{
    public async Task WaitForAvailableTokenAsync(bool isHighPriority, CancellationToken ct)
    {
        await queue.WaitForAvailableTokensAsync(isHighPriority, ct);
    }
}