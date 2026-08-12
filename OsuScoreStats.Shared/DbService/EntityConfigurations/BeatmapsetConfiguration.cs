using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsuScoreStats.Shared.DbService.Entities;

namespace OsuScoreStats.Shared.DbService.EntityConfigurations;

public class BeatmapsetConfiguration : IEntityTypeConfiguration<Beatmapset>
{
    public void Configure(EntityTypeBuilder<Beatmapset> builder)
    {
        builder
            .HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId);
    }
}