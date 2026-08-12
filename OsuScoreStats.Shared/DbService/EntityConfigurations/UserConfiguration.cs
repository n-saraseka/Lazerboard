using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsuScoreStats.Shared.DbService.Entities;

namespace OsuScoreStats.Shared.DbService.EntityConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .HasOne(u => u.Country)
            .WithMany()
            .HasForeignKey(u => u.CountryCode);
    }
}