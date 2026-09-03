using Lazerboard.Data.Database;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.DiscordHooks.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

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
                    .CommandTimeout(300))
            .UseSnakeCaseNamingConvention());

// Hook services
builder.Services.AddHostedService<GetLatestScannedBeatmapService>();

// Logs
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger());

var app = builder.Build();

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