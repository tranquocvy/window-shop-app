using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechHaven.Domain.Entities;

namespace TechHaven.Infrastructure.Persistence.Configurations;

public class CommissionConfiguration : IEntityTypeConfiguration<Commission>
{
  public void Configure(EntityTypeBuilder<Commission> builder)
  {
    builder.HasKey(e => e.CommissionId);

    builder.Property(e => e.UserId)
        .IsRequired();

    builder.Property(e => e.Month)
        .IsRequired();

    builder.Property(e => e.Year)
        .IsRequired();

    builder.Property(e => e.TotalSales)
        .IsRequired()
        .HasColumnType("decimal(18,2)");

    builder.Property(e => e.CommissionRate)
        .IsRequired()
        .HasColumnType("decimal(5,2)");

    builder.Property(e => e.Note)
        .HasMaxLength(255);

    builder.Property(e => e.CreatedAt)
        .IsRequired();

    // CommissionAmount is a computed property and should not be mapped
    builder.Ignore(e => e.CommissionAmount);

    builder.HasIndex(e => e.UserId);
    builder.HasIndex(e => new { e.UserId, e.Month, e.Year })
        .IsUnique();
    builder.HasIndex(e => new { e.Year, e.Month });

    // User -> Commission (One-to-Many)
    builder.HasOne(c => c.User)
        .WithMany(u => u.Commissions!)
        .HasForeignKey(c => c.UserId)
        .OnDelete(DeleteBehavior.Cascade); // Cascade delete commissions when user is deleted
  }
}