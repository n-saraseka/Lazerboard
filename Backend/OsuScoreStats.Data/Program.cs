using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using OsuScoreStats.Data.Database;
using OsuScoreStats.Data.OsuEntities.Enums;

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
                    .MapEnum<BeatmapStatus>("beatmap_status"))
            .UseSnakeCaseNamingConvention());

var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ScoreDataContext>();
await db.Database.MigrateAsync();