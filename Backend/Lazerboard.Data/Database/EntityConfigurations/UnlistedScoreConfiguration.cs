using Lazerboard.Data.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazerboard.Data.Database.EntityConfigurations;

public class UnlistedScoreConfiguration : IEntityTypeConfiguration<UnlistedScore>
{
    public void Configure(EntityTypeBuilder<UnlistedScore> builder)
    {
        builder
            .HasOne(s => s.Beatmap)
            .WithMany()
            .HasForeignKey(s => s.BeatmapId);
        builder
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId);
        builder
            .HasIndex(s => s.ScoreSource);
        builder.ComplexProperty(s => s.Statistics, stats => stats.ToJson());
    }
}