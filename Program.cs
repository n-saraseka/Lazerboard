using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService;
using OsuScoreStats.Calculations;
using OsuScoreStats.ScoreFetcher;
using OsuScoreStats.Api;
using OsuScoreStats.DbService.Repositories;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.Migrations;
using OsuScoreStats.OsuApi;
using OsuScoreStats.OsuApi.Enums;
using OsuScoreStats.OsuEntityToDtoService;

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

builder.Services.AddHttpClient();

// Database related
builder.Services.AddScoped<IBeatmapRepository, BeatmapRepository>();
builder.Services.AddScoped<IBeatmapsetRepository, BeatmapsetRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IScoreRepository, ScoreRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOsuEntityToDtoService, OsuEntityToDtoService>();
builder.Services.AddScoped<IBackpopulator, Backpopulator>();

// Score fetching related
builder.Services.AddScoped<OsuApiService>();
builder.Services.AddScoped<ICalculator, ScoreCalculator>();
builder.Services.AddScoped<IApiFetcher, ApiFetcher>();
builder.Services.AddScoped<IScoreProcessor, ScoreProcessor>();
builder.Services.AddScoped<IDataProcessor, DataProcessor>();
builder.Services.AddScoped<ICacheStore, CacheStore>();

// Background services
builder.Services.AddHostedService<ScoreLeaderboardService>();

// API
builder.Services.AddScoped<ScoreMethods>();
builder.Services.AddScoped<BeatmapMethods>();
builder.Services.AddScoped<UserMethods>();

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ScoreDataContext>();
    await db.Database.MigrateAsync();
    var backpopulator = scope.ServiceProvider.GetRequiredService<IBackpopulator>();
    var cancellationToken = CancellationToken.None;
    await backpopulator.BackpopulateAsync(cancellationToken);
}

if (!Directory.Exists(builder.Configuration["CacheFolder"]))
{
    Directory.CreateDirectory(builder.Configuration["CacheFolder"]);
}

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
        int? page,
        string? sort,
        bool isDesc,
        CancellationToken ct) => await scoreMethods.GetScoresAsync(
        mode, dateStart, dateEnd, country, mandatoryMods, optionalMods, amount, page, sort, isDesc, ct))
    .WithName("GetScores");

app.MapGet("/api/beatmaps/{id:int}", async (
        BeatmapMethods beatmapMethods, 
        int id,
        CancellationToken ct) => await beatmapMethods.GetBeatmapAsync(id, ct))
    .WithName("GetBeatmap");

app.MapGet("/api/beatmaps", async (
        BeatmapMethods beatmapMethods, 
        int[] beatmapIds,
        CancellationToken ct) => await beatmapMethods.GetBeatmapsAsync(beatmapIds, ct))
    .WithName("GetBeatmaps");

app.MapGet("/api/beatmapsets", async (
        BeatmapMethods beatmapMethods, 
        int[] beatmapsetIds,
        CancellationToken ct) => await beatmapMethods.GetBeatmapsetsAsync(beatmapsetIds, ct))
    .WithName("GetBeatmapsets");

app.MapGet("/api/users/{id:int}", async (
        UserMethods userMethods,
        int id,
        CancellationToken ct) => await userMethods.GetUserAsync(id, ct))
    .WithName("GetUser");

app.MapGet("/api/users", async (
        UserMethods userMethods, 
        int[] userIds,
        CancellationToken ct) => await userMethods.GetUsersAsync(userIds, ct))
    .WithName("GetUsers");

app.MapGet("/api/users/{userId:int}/scores", async (
        UserMethods userMethods,
        int userId,
        Mode? mode,
        DateOnly? dateStart,
        DateOnly? dateEnd,
        string[]? mandatoryMods,
        string[]? optionalMods,
        int? amount,
        int? page,
        string? sort,
        bool isDesc,
        CancellationToken ct) => await userMethods.GetUserScoresAsync(
        userId, mode, dateStart, dateEnd, mandatoryMods, optionalMods, amount, page, sort, isDesc, ct))
    .WithName("GetUserScores");

app.MapGet("/api/users/{id:int}/scores/count", async (
        UserMethods userMethods,
        int id,
        Mode? mode,
        CancellationToken ct) => await userMethods.GetUserScoresCountAsync(id, mode, ct))
    .WithName("GetUserScoresCount");

app.MapGet("/api/beatmaps/{id:int}/scores", async (
        BeatmapMethods beatmapMethods,
        int id,
        CancellationToken ct) => await beatmapMethods.GetBeatmapScoresAsync(id, ct))
    .WithName("GetBeatmapScores");

app.MapControllerRoute(
    name: "user",
    pattern: "user/{id}",
    defaults: new { controller = "User", action = "UserPage" });

app.MapControllerRoute(
    name: "index",
    pattern: "/",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "beatmapset",
    pattern: "/beatmapsets/{id:int}",
    defaults: new { controller = "Beatmapset", action = "BeatmapsetPage"});

app.MapControllerRoute(
    name: "beatmap",
    pattern: "/b/{id:int}",
    defaults: new { controller = "Beatmapset", action = "BeatmapPage"});

app.MapControllers();
app.Run();