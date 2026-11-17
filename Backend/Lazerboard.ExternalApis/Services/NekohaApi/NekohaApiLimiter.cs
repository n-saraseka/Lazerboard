namespace Lazerboard.ExternalApis.Services.NekohaApi;

public class NekohaApiLimiter(NekohaApiQueue queue) : ICentralizedRateLimiter
{
    public async Task WaitForAvailableTokenAsync(decimal priority, CancellationToken ct)
    {
        await queue.WaitForAvailableTokensAsync(priority, ct);
    }

    public async Task EnqueueAndShiftTheRestAsync(CancellationToken ct)
    {
        await queue.EnqueueAndShiftTheRestAsync(ct);
    }
}