using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using OsuScoreStats.BackgroundServices;
using OsuScoreStats.Shared.Calculations;
using OsuScoreStats.Shared.DbService;
using OsuScoreStats.Shared.DbService.Repositories;
using OsuScoreStats.Shared.DbService.Repositories.Interfaces;
using OsuScoreStats.Shared.Migrations;
using OsuScoreStats.Shared.OsuApi;
using OsuScoreStats.Shared.OsuApi.Enums;
using OsuScoreStats.Shared.OsuEntityToDtoService;
using OsuScoreStats.Shared.Processing;
using Serilog;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

// Background services
builder.Services.AddHostedService<ScoreFetcherService>();

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Logs
builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
    );

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "default",
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 60,
                SegmentsPerWindow = 10,
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
        await context.HttpContext.Response.WriteAsync($"Too many requests. Please try again later.", cancellationToken);
    };
});

var app = builder.Build();

app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStatusCodePagesWithReExecute("/{0}");
app.UseExceptionHandler("/500");

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();

app.MapControllerRoute(
    name: "user",
    pattern: "users/{id}",
    defaults: new { controller = "User", action = "UserPage" });

app.MapControllerRoute(
    name: "index",
    pattern: "/",
    defaults: new { controller = "General", action = "Index" });

app.MapControllerRoute(
    name: "about",
    pattern: "/about",
    defaults: new { controller = "General", action = "About" });

app.MapControllerRoute(
    name: "beatmapset",
    pattern: "/beatmapsets/{id:int}",
    defaults: new { controller = "Beatmapset", action = "BeatmapsetPage"});

app.MapControllerRoute(
    name: "beatmap",
    pattern: "/b/{id:int}",
    defaults: new { controller = "Beatmapset", action = "BeatmapPage"});

app.MapControllerRoute(
    name: "scoreranking",
    pattern: "scoreranking",
    defaults: new { controller = "ScoreRanking", action = "ScoreRanking"});

app.MapControllerRoute(
    name: "maniamillions",
    pattern: "maniamillions",
    defaults: new { controller = "ScoreRanking", action = "ManiaMillions"});

app.MapControllers();

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
