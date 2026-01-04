using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechHaven.Domain.Entities;

namespace TechHaven.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
  public void Configure(EntityTypeBuilder<Order> builder)
  {
    builder.HasKey(e => e.OrderId);

    builder.Property(e => e.UserId)
        .IsRequired();

    builder.Property(e => e.OrderDate)
        .IsRequired();

    builder.Property(e => e.Status)
        .IsRequired()
        .HasConversion<int>();

    builder.Property(e => e.SubtotalAmount)
        .IsRequired()
        .HasColumnType("decimal(18,2)");

    builder.Property(e => e.Discount)
        .HasColumnType("decimal(18,2)")
        .HasDefaultValue(0);

    builder.Property(e => e.TotalAmount)
        .IsRequired()
        .HasColumnType("decimal(18,2)");

    builder.Property(e => e.Notes)
        .HasMaxLength(1000)
        .HasColumnType("text");

    builder.HasIndex(e => e.CustomerId);
    builder.HasIndex(e => e.UserId);
    builder.HasIndex(e => e.OrderDate);
    builder.HasIndex(e => e.Status);
    builder.HasIndex(e => new { e.OrderDate, e.Status }); // Orders by date range queries

    // Customer -> Order (One-to-Many)
    builder.HasOne(o => o.Customer)
        .WithMany(c => c.Orders!)
        .HasForeignKey(o => o.CustomerId)
        .OnDelete(DeleteBehavior.SetNull); // If customer is deleted, set CustomerId to null

    // User -> Order (One-to-Many)
    builder.HasOne(o => o.User)
        .WithMany(u => u.Orders!)
        .HasForeignKey(o => o.UserId)
        .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of user with orders
  }
}