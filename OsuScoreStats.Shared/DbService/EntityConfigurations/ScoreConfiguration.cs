using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsuScoreStats.Shared.DbService.Entities;

namespace OsuScoreStats.Shared.DbService.EntityConfigurations;

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
            .HasIndex(s => s.Mode);
        builder
            .HasIndex(s => s.ModAcronyms);
        builder
            .HasIndex(s => s.SpeedChange);
        builder
            .HasIndex(s => s.Date);
        builder
            .HasIndex(s => s.PP);
    }
}