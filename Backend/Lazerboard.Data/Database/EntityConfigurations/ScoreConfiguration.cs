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
            .HasIndex(s => new { s.UserId, s.IsConvert, s.Mode, s.Date });
        // Global scores index
        builder
            .HasIndex(s => new { s.IsConvert, s.Mode, s.Date });
        // User graph indices
        builder
            .HasIndex(s => new { s.UserId, s.BeatmapId });
        builder
            .HasIndex(s => new { s.UserId, s.SpeedChange });
        builder
            .HasIndex(s => new { s.UserId, s.Mode, s.Rank });
        // Incoming scores index
        builder
            .HasIndex(s => new { s.BeatmapId, s.Mode, s.TotalScore, s.Date });
        // Filter specific indices
        builder
            .HasIndex(s => new { s.Mode, s.SpeedChange, s.UserId });
        builder
            .HasIndex(s => new { s.Mode, s.Accuracy, s.UserId });
        builder
            .HasIndex(s => new { s.Mode, s.PP, s.UserId });
        builder
            .HasIndex(s => s.ScoreSource);
        builder.ComplexProperty(s => s.Statistics, stats => stats.ToJson());
    }
}