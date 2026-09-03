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
            .WithMany()
            .HasForeignKey(s => s.BeatmapId);
        builder
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId);
        builder
            .HasIndex(s => s.ModAcronyms);
        builder
            .HasIndex(s => new { s.UserId, s.Date, s.Mode});
        builder
            .HasIndex(s => new { s.Date, s.Mode});
        builder
            .HasIndex(s => new { s.Rank, s.Mode });
        builder
            .HasIndex(s => new { s.TotalScore, s.Mode });
        builder
            .HasIndex(s => s.ScoreSource);
    }
}