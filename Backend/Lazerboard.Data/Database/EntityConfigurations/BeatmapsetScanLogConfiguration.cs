using Lazerboard.Data.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazerboard.Data.Database.EntityConfigurations;

public class BeatmapsetScanLogConfiguration : IEntityTypeConfiguration<BeatmapsetScanLog>
{
    public void Configure(EntityTypeBuilder<BeatmapsetScanLog> builder)
    {
        builder
            .HasIndex(b => b.EventType);
    }
}