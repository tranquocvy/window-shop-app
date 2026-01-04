using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechHaven.Domain.Entities;

namespace TechHaven.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
  public void Configure(EntityTypeBuilder<Role> builder)
  {
    builder.HasKey(e => e.RoleId);

    builder.Property(e => e.RoleName)
        .IsRequired()
        .HasMaxLength(100);

    builder.Property(e => e.Description)
        .HasMaxLength(500);

    builder.HasIndex(e => e.RoleName)
        .IsUnique();
  }
}