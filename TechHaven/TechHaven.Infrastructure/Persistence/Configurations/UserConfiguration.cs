using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechHaven.Domain.Entities;

namespace TechHaven.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.UserId);

        builder.Property(e => e.UserFullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Email)
            .HasMaxLength(150);

        builder.Property(e => e.UserName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.RoleId)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.HasSeenGuide)
            .HasDefaultValue(false);

        builder.Property(e => e.MfaEnabled)
            .HasDefaultValue(true);

        builder.Property(e => e.RefreshToken)
            .HasMaxLength(512);

        builder.Property(e => e.RefreshTokenExpiryTime);

        builder.Property(e => e.LastLoginAt);

        builder.HasIndex(e => e.UserName)
            .IsUnique();
        builder.HasIndex(e => e.RoleId);
        builder.HasIndex(e => new { e.IsActive, e.RoleId }); // Active users query

        // Role -> User (One-to-Many)
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users!)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of role with users
    }
}