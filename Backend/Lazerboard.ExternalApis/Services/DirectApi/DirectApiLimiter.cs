namespace Lazerboard.ExternalApis.Services.DirectApi;

public class DirectApiLimiter(DirectApiQueue queue) : ICentralizedRateLimiter
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