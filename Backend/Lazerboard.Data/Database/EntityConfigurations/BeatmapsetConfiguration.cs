using Lazerboard.Data.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazerboard.Data.Database.EntityConfigurations;

public class BeatmapsetConfiguration : IEntityTypeConfiguration<Beatmapset>
{
    public void Configure(EntityTypeBuilder<Beatmapset> builder)
    {
        builder
            .HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId);
        builder
            .HasIndex(b => b.MainStartedProcessingAt);
        builder
            .HasIndex(b => b.SecondaryStartedProcessingAt);
        builder
            .HasIndex(b => b.SecondaryStartedProcessingAt);
        builder
            .HasIndex(b => b.SecondaryFinishedProcessingAt);
        builder
            .HasIndex(b => b.StartedScanningAt);
        builder
            .HasIndex(b => b.FinishedScanningAt);
    }
}