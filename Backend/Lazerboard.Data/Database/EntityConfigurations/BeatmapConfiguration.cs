using Lazerboard.Data.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazerboard.Data.Database.EntityConfigurations;

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