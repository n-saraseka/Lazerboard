using System.Threading.RateLimiting;

namespace Lazerboard.ExternalApis.Services.DirectApi;

public class DirectApiLimiter : ICentralizedRateLimiter
{
    private readonly RateLimiter _rateLimiter;
    
    public DirectApiLimiter(IConfiguration config)
    {
        var externalApisConfig = config.GetSection("ExternalApis");
        var osuApiConfig = externalApisConfig.GetSection("OsuDotDirect");
        var apiInterval = osuApiConfig.GetValue<double>("ApiInterval");
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