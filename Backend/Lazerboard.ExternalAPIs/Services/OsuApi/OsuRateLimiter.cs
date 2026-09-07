using System.Threading.RateLimiting;

namespace Lazerboard.ExternalAPIs.Services.OsuApi;


public class OsuRateLimiter : ICentralizedRateLimiter
{
    private readonly RateLimiter _rateLimiter;

    public OsuRateLimiter(IConfiguration config)
    {
        var apiConfig = config.GetSection("OsuApi");
        var apiInterval = apiConfig.GetValue<double>("ApiInterval");
        _rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            AutoReplenishment = true,
            ReplenishmentPeriod = TimeSpan.FromSeconds(apiInterval),
            TokenLimit = 1,
            TokensPerPeriod = 1,
            QueueLimit = 10,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    }

    public async Task WaitForAvailableTokenAsync(CancellationToken ct)
    {
        using var lease = await _rateLimiter.AcquireAsync(1, ct);
    }
}