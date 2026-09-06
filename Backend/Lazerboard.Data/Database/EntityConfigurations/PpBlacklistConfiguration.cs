using Lazerboard.Data.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazerboard.Data.Database.EntityConfigurations;

public class PpBlacklistConfiguration : IEntityTypeConfiguration<PpBlacklistBeatmap>
{
    public void Configure(EntityTypeBuilder<PpBlacklistBeatmap> builder)
    {
        builder
            .HasOne(b => b.Beatmap)
            .WithOne()
            .HasForeignKey<PpBlacklistBeatmap>(b => b.BeatmapId);
    }
}