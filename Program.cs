using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService;
using OsuScoreStats.Calculators;
using OsuScoreStats.ScoreFetcherService;
using System.Threading.RateLimiting;
using OsuScoreStats.ApiMethods;
using OsuScoreStats.OsuApi;
using OsuScoreStats.OsuApi.Enums;
using OsuScoreStats.OsuEntityToDtoService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddPooledDbContextFactory<ScoreDataContext>(
    opt =>
    opt.UseNpgsql(
            builder.Configuration["DefaultConnection"],
            o => o
                .MapEnum<Mode>("mode")
                .MapEnum<Grade>("grade")
                .MapEnum<BeatmapStatus>("beatmap_status"))
                .UseSnakeCaseNamingConvention()
                .EnableSensitiveDataLogging());

builder.Services.AddHttpClient();

builder.Services.AddSingleton<OsuApiService>();
builder.Services.AddSingleton<ICalculator, ScoreCalculator>();
builder.Services.AddSingleton<IScoreFetcher, ScoreFetcher>();
builder.Services.AddSingleton<IOsuEntityToDtoService, OsuEntityToDtoService>();
builder.Services.AddHostedService<LeaderboardWorker>();
builder.Services.AddScoped<ScoreMethods>();
builder.Services.AddScoped<BeatmapMethods>();
builder.Services.AddScoped<UserMethods>();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

var contextFactory = app.Services.GetRequiredService<IDbContextFactory<ScoreDataContext>>();
using var context = contextFactory.CreateDbContext();
context.Database.EnsureCreated();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();

app.MapGet("/api/scores", async (
        ScoreMethods scoreMethods, 
        Mode? mode, 
        DateOnly? dateStart,
        DateOnly? dateEnd,
        string? country,
        string[]? mandatoryMods,
        string[]? optionalMods,
        int? amount,
        int? userId,
        string? sort,
        bool isDesc,
        CancellationToken ct) => await scoreMethods.GetScoresAsync(
        mode, dateStart, dateEnd, country, mandatoryMods, optionalMods, amount, userId, sort, isDesc, ct))
    .WithName("GetScores")
    .WithOpenApi();

app.MapGet("/api/beatmaps/{id:int}", async (
        BeatmapMethods beatmapMethods, 
        int id,
        CancellationToken ct) => await beatmapMethods.GetBeatmapAsync(id, ct))
    .WithName("GetBeatmap")
    .WithOpenApi();

app.MapGet("/api/beatmaps", async (
        BeatmapMethods beatmapMethods, 
        int[] beatmapIds,
        CancellationToken ct) => await beatmapMethods.GetBeatmapsAsync(beatmapIds, ct))
    .WithName("GetBeatmaps")
    .WithOpenApi();

app.MapGet("/api/beatmapsets", async (
        BeatmapMethods beatmapMethods, 
        int[] beatmapsetIds,
        CancellationToken ct) => await beatmapMethods.GetBeatmapsetsAsync(beatmapsetIds, ct))
    .WithName("GetBeatmapsets")
    .WithOpenApi();

app.MapGet("/api/users/{id:int}", async (
        UserMethods userMethods,
        int id,
        CancellationToken ct) => await userMethods.GetUserAsync(id, ct))
    .WithName("GetUser")
    .WithOpenApi();

app.MapGet("/api/users", async (
        UserMethods userMethods, 
        int[] userIds,
        CancellationToken ct) => await userMethods.GetUsersAsync(userIds, ct))
    .WithName("GetUsers")
    .WithOpenApi();

app.MapGet("/api/users/{id:int}/scores/count", async (
        UserMethods userMethods,
        int id,
        Mode? mode,
        CancellationToken ct) => await userMethods.GetUserScoresCountAsync(id, mode, ct))
    .WithName("GetUserScoresCount")
    .WithOpenApi();

app.MapControllerRoute(
    name: "user",
    pattern: "user/{id}",
    defaults: new { controller = "User", action = "Index" });

app.MapControllers();
app.Run();