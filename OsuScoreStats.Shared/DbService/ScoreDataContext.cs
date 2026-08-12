using Microsoft.EntityFrameworkCore;
using OsuScoreStats.Shared.DbService.Entities;
using OsuScoreStats.Shared.DbService.EntityConfigurations;

namespace OsuScoreStats.Shared.DbService;

public class ScoreDataContext(DbContextOptions<ScoreDataContext> options) : DbContext(options)
{
    public DbSet<Beatmap> Beatmaps { get; set; }
    public DbSet<Beatmapset> Beatmapsets { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Score> Scores { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ScoreConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new BeatmapConfiguration());
    }
}
    