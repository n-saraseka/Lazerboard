using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsuScoreStats.Data.Database.Entities;

namespace OsuScoreStats.Data.Database.EntityConfigurations;

public class ScorePendingDeletionConfiguration : IEntityTypeConfiguration<ScorePendingDeletion>
{
    public void Configure(EntityTypeBuilder<ScorePendingDeletion> builder)
    {
        builder
            .HasOne(s => s.Score)
            .WithOne()
            .HasForeignKey<ScorePendingDeletion>(s => s.ScoreId);
    }
}