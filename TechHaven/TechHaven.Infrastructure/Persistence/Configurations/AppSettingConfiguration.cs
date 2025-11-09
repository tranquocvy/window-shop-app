using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechHaven.Domain.Entities;

namespace TechHaven.Infrastructure.Persistence.Configurations;

public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
  public void Configure(EntityTypeBuilder<AppSetting> builder)
  {
    builder.HasKey(e => e.AppSettingId);

    builder.Property(e => e.Key)
        .IsRequired()
        .HasMaxLength(100);

    builder.Property(e => e.Value)
        .HasMaxLength(1000);

    builder.Property(e => e.ValueType)
        .IsRequired()
        .HasConversion<int>();

    builder.Property(e => e.Category)
        .HasMaxLength(50);

    builder.Property(e => e.Description)
        .HasMaxLength(255);

    builder.Property(e => e.IsSystem)
        .IsRequired()
        .HasDefaultValue(false);

    builder.Property(e => e.UpdatedAt)
        .IsRequired();

    builder.HasIndex(e => e.Key)
        .IsUnique();
    builder.HasIndex(e => e.Category);
  }
}