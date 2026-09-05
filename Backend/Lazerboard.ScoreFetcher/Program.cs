using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Lazerboard.Data.Database;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.Redis.Repositories;
using Lazerboard.Data.Redis.Repositories.Interfaces;
using Lazerboard.ScoreFetcher.BackgroundServices;
using Lazerboard.ScoreFetcher.Calculations;
using Lazerboard.ScoreFetcher.OsuApi;
using Lazerboard.ScoreFetcher.OsuEntityToDtoService;
using Lazerboard.ScoreFetcher.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder();

var dbConfig = builder.Configuration.GetSection("Database");

var connectionString = new NpgsqlConnectionStringBuilder
{
    Host = dbConfig["Host"],
    Port = int.Parse(dbConfig["Port"]),
    Database = dbConfig["Database"],
    Username = dbConfig["Username"],
    Password = dbConfig["Password"],
};

builder.Services.AddDbContext<ScoreDataContext>(
    opt =>
        opt.UseNpgsql(
                connectionString.ConnectionString,
                o => o
                    .MapEnum<Mode>("mode")
                    .MapEnum<Grade>("grade")
                    .MapEnum<BeatmapStatus>("beatmap_status")
                    .MapEnum<ScoreSource>("score_source")
                    .CommandTimeout(120))
            .UseSnakeCaseNamingConvention());

// Database related
builder.Services.AddScoped<IBeatmapRepository, BeatmapRepository>();
builder.Services.AddScoped<IBeatmapsetRepository, BeatmapsetRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IScoreRepository, ScoreRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IScorePendingDeletionRepository, ScorePendingDeletionRepository>();
builder.Services.AddScoped<IOsuEntityToDtoService, OsuEntityToDtoService>();
builder.Services.AddScoped<IBackpopulator, Backpopulator>();

// Score fetching related
builder.Services.AddScoped<ICalculator, ScoreCalculator>();
builder.Services.AddScoped<IApiFetcher, ApiFetcher>();
builder.Services.AddScoped<IScoreProcessor, ScoreProcessor>();
builder.Services.AddScoped<IDataProcessor, DataProcessor>();
builder.Services.AddScoped<IScoreFetchingUtils, ScoreFetchingUtils>();

builder.Services.AddSingleton<ICentralizedRateLimiter, CentralizedRateLimiter>();
builder.Services.AddSingleton<ISeedingState, SeedingState>();

// Caching
var redisConfig = builder.Configuration.GetSection("Redis");
var host = redisConfig["Host"];
var username = redisConfig["Username"];
var password = redisConfig["Password"];
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => 
    ConnectionMultiplexer.Connect(new ConfigurationOptions
    {
        EndPoints = { { host, 6379 } },
        User = username,
        Password = password
    }));
builder.Services.AddScoped<IBeatmapCacheRepository, BeatmapCacheRepository>();
builder.Services.AddScoped<IScoreCacheRepository, ScoreCacheRepository>();
builder.Services.AddSingleton<ICacheStore, CacheStore>();

// HTTP Client
builder.Services.AddHttpClient<OsuApiService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddResilienceHandler("Retry", (resilienceBuilder, context) =>
    {
        resilienceBuilder.AddRetry(new HttpRetryStrategyOptions
        {
            ShouldHandle = static args => args.Outcome switch
            {
                { Result: { IsSuccessStatusCode: false, StatusCode: not HttpStatusCode.UnprocessableEntity } } => PredicateResult.True(),
                _ => PredicateResult.False()
            },
            
            MaxRetryAttempts = 5,
            Delay = TimeSpan.FromSeconds(5),
            
            OnRetry = args =>
            {
                var logger = context.ServiceProvider.GetRequiredService<ILogger<OsuApiService>>();
                
                logger.Log(LogLevel.Warning, args.Outcome.Exception ,"HTTP request error for URL: {@requestURL} (status code: {statusCode}). Retry no. {attempt}. Next retry in {timespan}", 
                    args.Outcome.Result?.RequestMessage?.RequestUri, args.Outcome.Result?.StatusCode, args.AttemptNumber, args.RetryDelay);
                
                return default;
            }
        });
    });

// Background services
builder.Services.AddHostedService<LeaderboardSeedingService>();
builder.Services.AddHostedService<FirehoseService>();
builder.Services.AddHostedService<ScoresCountService>();

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "default",
            factory: partition => new TokenBucketRateLimiterOptions
            {
                AutoReplenishment = true,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                TokenLimit = 15,
                TokensPerPeriod = 1,
                QueueLimit = 3,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            }
        ));
    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers["Retry-After"] = $"{(int)retryAfter.TotalSeconds}";
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken);
    };
});

// Logs
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger());

builder.Services.AddControllers();

var app = builder.Build();

app.UseRateLimiter();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var backpopulator = scope.ServiceProvider.GetRequiredService<IBackpopulator>();
    var cancellationToken = CancellationToken.None;
    await backpopulator.BackpopulateAsync(cancellationToken);
    var cacheStore = scope.ServiceProvider.GetRequiredService<ICacheStore>();
    try
    {
        await cacheStore.CleanupCacheAsync();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Could not cleanup beatmap cache on startup");
    }
}

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}