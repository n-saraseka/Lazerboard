using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Lazerboard.Data.Database;
using Lazerboard.Data.Database.Repositories;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.ScoreFetcher.BackgroundServices;
using Lazerboard.ScoreFetcher.Calculations;
using Lazerboard.ScoreFetcher.Jobs;
using Lazerboard.ScoreFetcher.OsuApi;
using Lazerboard.ScoreFetcher.OsuEntityToDtoService;
using Lazerboard.ScoreFetcher.Processing;
using Polly;
using Polly.Extensions.Http;
using Quartz;
using Serilog;

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

// Quartz job for removing restricted user scores and updating usernames
builder.Services.AddQuartz(q =>
{
    q.ScheduleJob<UpdateUserAndScoreDataJob>(trigger => trigger
        .WithIdentity("User and score check job")
        .WithSchedule(CronScheduleBuilder
            .DailyAtHourAndMinute(0, 0)
            .InTimeZone(TimeZoneInfo.Utc)));
});

// Logs
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger());

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
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