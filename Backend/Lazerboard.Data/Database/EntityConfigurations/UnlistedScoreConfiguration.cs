using Lazerboard.Data.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazerboard.Data.Database.EntityConfigurations;

public class UnlistedScoreConfiguration : IEntityTypeConfiguration<UnlistedScore>
{
    public void Configure(EntityTypeBuilder<UnlistedScore> builder)
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
    }
}