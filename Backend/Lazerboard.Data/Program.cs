using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Lazerboard.Data.Database;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.Services;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateApplicationBuilder(args);

var dbConfig = builder.Configuration.GetSection("Database");

var connectionString = new NpgsqlConnectionStringBuilder
{
    Host = dbConfig["Host"],
    Port = dbConfig.GetValue<int>("Port"),
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
                    .MapEnum<ScanEventType>("scan_event_type")
                    .MapEnum<ReportStatus>("report_status")
                    .MapEnum<CooldownReason>("cooldown_reason")
                    .MapEnum<UserGroup>("user_group")
                    .CommandTimeout(900))
            .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<MigrationService>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ScoreDataContext>();
await db.Database.MigrateAsync();
var additionalMigrations = scope.ServiceProvider.GetRequiredService<MigrationService>();
await additionalMigrations.SetAdminUserAsync();