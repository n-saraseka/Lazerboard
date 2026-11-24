using Lazerboard.Data.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazerboard.Data.Database.EntityConfigurations;

public class ScoreConfiguration : IEntityTypeConfiguration<Score>
{
    public void Configure(EntityTypeBuilder<Score> builder)
    {
        builder.PrimitiveCollection(s => s.ModAcronyms);
        builder
            .HasOne(s => s.Beatmap)
            .WithMany(b => b.Scores)
            .HasForeignKey(s => s.BeatmapId);
        builder
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId);
        builder
            .HasIndex(s => s.BeatmapId);
        // User scores index
        builder
            .HasIndex(s => new { s.UserId, s.Date });
        // Global scores index
        builder
            .HasIndex(s => new { s.Date });
        // User graph indexes
        builder
            .HasIndex(s => new { s.UserId, s.BeatmapId});
        builder
            .HasIndex(s => new { s.UserId, s.SpeedChange});
        builder
            .HasIndex(s => new { s.UserId, s.Rank});
        builder
            .HasIndex(s => new { s.UserId, s.Mode});
        // Filter specific indexes
        builder
            .HasIndex(s => new { s.SpeedChange, s.UserId });
        builder
            .HasIndex(s => new { s.Accuracy, s.UserId });
        builder
            .HasIndex(s => new { s.PP, s.UserId });
        builder
            .HasIndex(s => new { s.BeatmapId, s.UserId });
        builder
            .HasIndex(s => new { s.Rank, s.UserId });
        builder
            .HasIndex(s => new { s.Mode, s.UserId });
        builder.ComplexProperty(s => s.Statistics, stats => stats.ToJson());
    }
}