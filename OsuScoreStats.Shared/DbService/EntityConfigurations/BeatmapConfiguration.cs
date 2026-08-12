using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsuScoreStats.Shared.DbService.Entities;

namespace OsuScoreStats.Shared.DbService.EntityConfigurations;

public class BeatmapConfiguration : IEntityTypeConfiguration<Beatmap>
{
    public void Configure(EntityTypeBuilder<Beatmap> builder)
    {
        builder
            .HasOne(b => b.Beatmapset)
            .WithMany()
            .HasForeignKey(b => b.BeatmapsetId);
    }
}