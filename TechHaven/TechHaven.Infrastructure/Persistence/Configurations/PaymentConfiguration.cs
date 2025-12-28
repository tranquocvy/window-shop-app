using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechHaven.Domain.Entities;

namespace TechHaven.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
  public void Configure(EntityTypeBuilder<Payment> builder)
  {
    builder.HasKey(e => e.PaymentId);

    builder.Property(e => e.OrderId)
        .IsRequired();

    builder.Property(e => e.PaymentMethod)
        .IsRequired()
        .HasConversion<int>();

    builder.Property(e => e.Amount)
        .IsRequired()
        .HasColumnType("decimal(18,2)");

    builder.Property(e => e.PaymentDate)
        .IsRequired();

    builder.HasIndex(e => e.OrderId);
    builder.HasIndex(e => e.PaymentDate);

    // Order -> Payment (One-to-Many)
    builder.HasOne(p => p.Order)
        .WithMany(o => o.Payments!)
        .HasForeignKey(p => p.OrderId)
        .OnDelete(DeleteBehavior.Cascade); // Cascade delete payments when order is deleted
  }
}