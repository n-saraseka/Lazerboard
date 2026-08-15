using System.Threading.RateLimiting;

namespace OsuScoreStats.OsuApi;


public class CentralizedRateLimiter : ICentralizedRateLimiter
{
    private readonly RateLimiter _rateLimiter;

    public CentralizedRateLimiter(IConfiguration config)
    {
        var apiConfig = config.GetSection("OsuApi");
        var apiInterval = apiConfig.GetValue<double>("ApiInterval");
        _rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            AutoReplenishment = true,
            ReplenishmentPeriod = TimeSpan.FromSeconds(apiInterval),
            TokenLimit = 1,
            TokensPerPeriod = 1,
            // This way, a situation where two requests from different services occur at the same second can't happen.
            QueueLimit = 1,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    }

    public async Task WaitForAvailableTokenAsync(CancellationToken ct)
    {
        using var lease = await _rateLimiter.AcquireAsync(1, ct);
    }
}