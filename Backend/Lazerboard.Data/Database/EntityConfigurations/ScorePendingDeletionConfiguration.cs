using Lazerboard.Data.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazerboard.Data.Database.EntityConfigurations;

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