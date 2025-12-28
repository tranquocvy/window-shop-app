using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechHaven.Domain.Entities;

namespace TechHaven.Infrastructure.Persistence.Configurations;

public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
  public void Configure(EntityTypeBuilder<OrderDetail> builder)
  {
    builder.HasKey(e => e.OrderDetailId);

    builder.Property(e => e.OrderId)
        .IsRequired();

    builder.Property(e => e.ProductId)
        .IsRequired();

    builder.Property(e => e.Quantity)
        .IsRequired();

    builder.Property(e => e.UnitPrice)
        .IsRequired()
        .HasColumnType("decimal(18,2)");

    // SubTotal is a computed property and should not be mapped
    builder.Ignore(e => e.SubTotal);

    builder.HasIndex(e => e.OrderId);
    builder.HasIndex(e => e.ProductId);

    // Composite index for Order-Product uniqueness (one product per order line)
    builder.HasIndex(e => new { e.OrderId, e.ProductId });
    
    // Order -> OrderDetail (One-to-Many)
    builder.HasOne(od => od.Order)
        .WithMany(o => o.OrderDetails!)
        .HasForeignKey(od => od.OrderId)
        .OnDelete(DeleteBehavior.Cascade); // Cascade delete order details when order is deleted

    // Product -> OrderDetail (One-to-Many)
    builder.HasOne(od => od.Product)
        .WithMany(p => p.OrderDetails!)
        .HasForeignKey(od => od.ProductId)
        .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of product with order details
  }
}