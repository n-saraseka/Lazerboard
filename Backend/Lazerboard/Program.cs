using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Lazerboard.Data.Database;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

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
                .CommandTimeout(60))
                .UseSnakeCaseNamingConvention());

// Database related
builder.Services.AddScoped<IBeatmapRepository, BeatmapRepository>();
builder.Services.AddScoped<IBeatmapsetRepository, BeatmapsetRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IScoreRepository, ScoreRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddRazorComponents();
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
