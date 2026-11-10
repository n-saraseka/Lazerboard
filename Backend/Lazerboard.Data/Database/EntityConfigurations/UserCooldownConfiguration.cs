using Lazerboard.Data.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazerboard.Data.Database.EntityConfigurations;

public class UserCooldownConfiguration : IEntityTypeConfiguration<UserCooldown>
{
    public void Configure(EntityTypeBuilder<UserCooldown> builder)
    {
        builder
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId);
    }
}