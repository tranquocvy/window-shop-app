using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechHaven.Domain.Entities;

namespace TechHaven.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(e => e.ProductId);

        builder.Property(e => e.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.BrandName)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue(string.Empty);

        builder.Property(e => e.Color)
            .HasMaxLength(50);

        builder.Property(e => e.Processor)
            .HasMaxLength(100);

        builder.Property(e => e.ScreenSize)
            .HasColumnType("decimal(5,2)");

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(300);

        builder.Property(e => e.ImageGalleryJson)
            .HasColumnType("text");

        builder.Property(e => e.CostPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.SellPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.StockQuantity)
            .HasDefaultValue(0);

        builder.Property(e => e.Description)
            .HasColumnType("text");

        builder.Property(e => e.IsDraft)
            .HasDefaultValue(false);

        builder.HasIndex(e => e.ProductName);
        builder.HasIndex(e => e.BrandName);
        builder.HasIndex(e => new { e.IsDraft }); // Products search optimization
    }
}