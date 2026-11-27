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
        // User scores index
        builder
            .HasIndex(s => new { s.UserId, s.Date, s.Mode, s.IsConvert });
        // Global scores index
        builder
            .HasIndex(s => new { s.Date, s.Mode, s.IsConvert });
        // Filter specific indexes
        builder
            .HasIndex(s => new { s.SpeedChange, s.UserId, s.Mode, s.IsConvert });
        builder
            .HasIndex(s => new { s.PP, s.UserId, s.Mode, s.IsConvert });
        builder
            .HasIndex(s => new { s.BeatmapId, s.UserId, s.Mode, s.IsConvert });
        builder
            .HasIndex(s => new { s.Accuracy, s.UserId, s.Mode, s.IsConvert });
        builder
            .HasIndex(s => new { s.Rank, s.UserId, s.Mode, s.IsConvert });
        builder
            .HasIndex(s => new { s.Mode, s.UserId, s.IsConvert });
        builder.ComplexProperty(s => s.Statistics, stats => stats.ToJson());
    }
}