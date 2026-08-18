using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsuScoreStats.Data.Database.Entities;

namespace OsuScoreStats.Data.Database.EntityConfigurations;

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