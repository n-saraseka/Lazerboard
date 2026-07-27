using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService;
using OsuScoreStats.Calculators;
using OsuScoreStats.ScoreFetcher;
using OsuScoreStats.ApiMethods;
using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.OsuApi;
using OsuScoreStats.OsuApi.Enums;
using OsuScoreStats.OsuEntityToDtoService;
using Sentry.Protocol;

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

// Score fetching related
builder.Services.AddScoped<OsuApiService>();
builder.Services.AddScoped<ICalculator, ScoreCalculator>();
builder.Services.AddScoped<IApiFetcher, ApiFetcher>();
builder.Services.AddScoped<IScoreProcessor, ScoreProcessor>();
builder.Services.AddScoped<IDataProcessor, DataProcessor>();

// Background services
builder.Services.AddHostedService<ScoreLeaderboardService>();

// API
builder.Services.AddScoped<ScoreMethods>();
builder.Services.AddScoped<BeatmapMethods>();
builder.Services.AddScoped<UserMethods>();

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ScoreDataContext>();
    await db.Database.EnsureCreatedAsync();
    await db.Database.MigrateAsync();
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
        string[]? mandatoryMods,
        string[]? optionalMods,
        int? amount,
        int? page,
        string? sort,
        bool isDesc,
        CancellationToken ct) => await userMethods.GetUserScoresAsync(
        userId, mode, mandatoryMods, optionalMods, amount, page, sort, isDesc, ct))
    .WithName("GetUserScores");

app.MapGet("/api/users/{id:int}/scores/count", async (
        UserMethods userMethods,
        int id,
        Mode? mode,
        CancellationToken ct) => await userMethods.GetUserScoresCountAsync(id, mode, ct))
    .WithName("GetUserScoresCount");

app.MapControllerRoute(
    name: "user",
    pattern: "user/{id}",
    defaults: new { controller = "User", action = "UserPage" });

app.MapControllers();
app.Run();