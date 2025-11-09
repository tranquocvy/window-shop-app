using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechHaven.Domain.Entities;

namespace TechHaven.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
  public void Configure(EntityTypeBuilder<Category> builder)
  {
    builder.HasKey(e => e.CategoryId);

    builder.Property(e => e.CategoryName)
        .IsRequired()
        .HasMaxLength(100);

    builder.Property(e => e.Description)
        .HasMaxLength(255);

    builder.HasIndex(e => e.CategoryName)
        .IsUnique();
  }
}