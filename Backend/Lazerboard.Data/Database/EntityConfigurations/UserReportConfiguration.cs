using Lazerboard.Data.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazerboard.Data.Database.EntityConfigurations;

public class UserReportConfiguration : IEntityTypeConfiguration<UserReport>
{
    public void Configure(EntityTypeBuilder<UserReport> builder)
    {
        builder
            .HasOne(r => r.ReportedUser)
            .WithMany()
            .HasForeignKey(r => r.ReportedUserId);
        builder
            .HasOne(r => r.ReportingUser)
            .WithMany()
            .HasForeignKey(r => r.ReportingUserId);
        builder
            .HasIndex(r => r.ReportedOn);
        builder
            .HasIndex(r => r.ReportedUserId)
            .HasFilter("status = 'in_progress'")
            .IsUnique();
    }
}