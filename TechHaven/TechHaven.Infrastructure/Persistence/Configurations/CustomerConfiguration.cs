using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechHaven.Domain.Entities;

namespace TechHaven.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
  public void Configure(EntityTypeBuilder<Customer> builder)
  {
    builder.HasKey(e => e.CustomerId);

    builder.Property(e => e.CustomerName)
        .IsRequired()
        .HasMaxLength(150);

    builder.Property(e => e.PhoneNumber)
        .IsRequired()
        .HasMaxLength(15);

    builder.Property(e => e.Email)
        .HasMaxLength(150);

    builder.Property(e => e.Address)
        .HasMaxLength(300);

    builder.Property(e => e.Type)
        .IsRequired()
        .HasConversion<int>(); // convert enum to int

    builder.Property(e => e.TotalPurchased)
        .HasColumnType("decimal(18,2)")
        .HasDefaultValue(0);

    builder.Property(e => e.Note)
        .HasMaxLength(255);

    builder.HasIndex(e => e.PhoneNumber);
    builder.HasIndex(e => e.Email);
  }
}
