using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OsuScoreStats.DatabaseSeeder;
using OsuScoreStats.Shared.Calculations;
using OsuScoreStats.Shared.DbService;
using OsuScoreStats.Shared.DbService.Repositories;
using OsuScoreStats.Shared.DbService.Repositories.Interfaces;
using OsuScoreStats.Shared.Migrations;
using OsuScoreStats.Shared.OsuApi;
using OsuScoreStats.Shared.OsuApi.Enums;
using OsuScoreStats.Shared.OsuEntityToDtoService;
using OsuScoreStats.Shared.Processing;
using Polly;
using Polly.Extensions.Http;
using Serilog;

var builder = Host.CreateApplicationBuilder();
builder.Services.AddDbContext<ScoreDataContext>(
    opt =>
        opt.UseNpgsql(
                builder.Configuration["DefaultConnection"],
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
builder.Services.AddScoped<ICacheStore, CacheStore>();
builder.Services.AddScoped<ScoreFetchingUtils>();

builder.Services.AddHostedService<LeaderboardFetcherService>();

// Logs
builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services);
});

var baseDirectory = Directory.GetCurrentDirectory();

if (!Directory.Exists($"{baseDirectory}/${builder.Configuration["CacheFolder"]}"))
{
    Directory.CreateDirectory($"{baseDirectory}/${builder.Configuration["CacheFolder"]}");
}

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