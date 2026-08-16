using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OsuScoreStats.Data.Database;
using OsuScoreStats.Data.Database.Repositories;
using OsuScoreStats.Data.Database.Repositories.Interfaces;
using OsuScoreStats.Data.OsuEntities.Enums;
using OsuScoreStats.ScoreFetcher.BackgroundServices;
using OsuScoreStats.ScoreFetcher.Calculations;
using OsuScoreStats.ScoreFetcher.OsuApi;
using OsuScoreStats.ScoreFetcher.OsuEntityToDtoService;
using OsuScoreStats.ScoreFetcher.Processing;
using Polly;
using Polly.Extensions.Http;
using Serilog;

var builder = Host.CreateApplicationBuilder();

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
                    .MapEnum<BeatmapStatus>("beatmap_status"))
            .UseSnakeCaseNamingConvention());

// Database related
builder.Services.AddScoped<IBeatmapRepository, BeatmapRepository>();
builder.Services.AddScoped<IBeatmapsetRepository, BeatmapsetRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IScoreRepository, ScoreRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOsuEntityToDtoService, OsuEntityToDtoService>();
builder.Services.AddScoped<IBackpopulator, Backpopulator>();

// Score fetching related
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider services)
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(4, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                var logger = services.GetRequiredService<ILogger<OsuApiService>>();
                
                logger.Log(LogLevel.Warning, outcome.Exception ,"HTTP request error. Retry no. {attempt}. Next retry in {timespan}", 
                    retryCount, timespan);
            });
}
builder.Services.AddHttpClient<OsuApiService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler((services, request) => GetRetryPolicy(services));
builder.Services.AddScoped<ICalculator, ScoreCalculator>();
builder.Services.AddScoped<IApiFetcher, ApiFetcher>();
builder.Services.AddScoped<IScoreProcessor, ScoreProcessor>();
builder.Services.AddScoped<IDataProcessor, DataProcessor>();
builder.Services.AddScoped<IScoreFetchingUtils, ScoreFetchingUtils>();

builder.Services.AddSingleton<ICentralizedRateLimiter, CentralizedRateLimiter>();
builder.Services.AddSingleton<ISeedingState, SeedingState>();
builder.Services.AddSingleton<ICacheStore, CacheStore>();

// Background services
builder.Services.AddHostedService<LeaderboardSeedingService>();
builder.Services.AddHostedService<FirehoseService>();

// Logs
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ScoreDataContext>();
    await db.Database.MigrateAsync();
    var backpopulator = scope.ServiceProvider.GetRequiredService<IBackpopulator>();
    var cancellationToken = CancellationToken.None;
    await backpopulator.BackpopulateAsync(cancellationToken);
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