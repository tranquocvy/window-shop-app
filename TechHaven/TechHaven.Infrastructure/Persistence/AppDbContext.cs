using Microsoft.EntityFrameworkCore;
using TechHaven.Domain.Entities;

namespace TechHaven.Infrastructure.Persistence;

/// <summary>
/// Represents the database context for the TechHaven application using Entity Framework Core.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by a DbContext.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Customers database set.
    /// </summary>
    public DbSet<Customer> Customers { get; set; }

    /// <summary>
    /// Gets or sets the Products database set.
    /// </summary>
    public DbSet<Product> Products { get; set; }

    /// <summary>
    /// Gets or sets the Categories database set.
    /// </summary>
    public DbSet<Category> Categories { get; set; }

    /// <summary>
    /// Gets or sets the Orders database set.
    /// </summary>
    public DbSet<Order> Orders { get; set; }

    /// <summary>
    /// Gets or sets the OrderDetails database set.
    /// </summary>
    public DbSet<OrderDetail> OrderDetails { get; set; }

    /// <summary>
    /// Gets or sets the Payments database set.
    /// </summary>
    public DbSet<Payment> Payments { get; set; }

    /// <summary>
    /// Gets or sets the Users database set.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Gets or sets the Roles database set.
    /// </summary>
    public DbSet<Role> Roles { get; set; }

    /// <summary>
    /// Gets or sets the Commissions database set.
    /// </summary>
    public DbSet<Commission> Commissions { get; set; }

    /// <summary>
    /// Gets or sets the AppSettings database set.
    /// </summary>
    public DbSet<AppSetting> AppSettings { get; set; }

    /// <summary>
    /// Configures the model that was discovered by convention from the entity types
    /// exposed in <see cref="DbSet{TEntity}"/> properties on your derived context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Customer entity
        ConfigureCustomer(modelBuilder);

        // Configure Product entity
        ConfigureProduct(modelBuilder);

        // Configure Category entity
        ConfigureCategory(modelBuilder);

        // Configure Order entity
        ConfigureOrder(modelBuilder);

        // Configure OrderDetail entity
        ConfigureOrderDetail(modelBuilder);

        // Configure Payment entity
        ConfigurePayment(modelBuilder);

        // Configure User entity
        ConfigureUser(modelBuilder);

        // Configure Role entity
        ConfigureRole(modelBuilder);

        // Configure Commission entity
        ConfigureCommission(modelBuilder);

        // Configure AppSetting entity
        ConfigureAppSetting(modelBuilder);

        // Configure relationships
        ConfigureRelationships(modelBuilder);

        // Configure indexes
        ConfigureIndexes(modelBuilder);
    }

    private void ConfigureCustomer(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Customer>();

        entity.HasKey(e => e.CustomerId);

        entity.Property(e => e.CustomerName)
            .IsRequired()
            .HasMaxLength(150);

        entity.Property(e => e.PhoneNumber)
            .IsRequired()
            .HasMaxLength(15);

        entity.Property(e => e.Email)
            .HasMaxLength(150);

        entity.Property(e => e.Address)
            .HasMaxLength(300);

        entity.Property(e => e.Type)
            .IsRequired()
            .HasConversion<int>(); // convert enum to int

        entity.Property(e => e.TotalPurchased)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        entity.Property(e => e.Note)
            .HasMaxLength(255);

        entity.HasIndex(e => e.PhoneNumber);
        entity.HasIndex(e => e.Email);
    }

    private void ConfigureProduct(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Product>();

        entity.HasKey(e => e.ProductId);

        entity.Property(e => e.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.CategoryId)
            .IsRequired();

        entity.Property(e => e.BrandName)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue(string.Empty);

        entity.Property(e => e.Color)
            .HasMaxLength(50);


        entity.Property(e => e.Processor)
            .HasMaxLength(100);

        entity.Property(e => e.ScreenSize)
            .HasColumnType("decimal(5,2)");

        entity.Property(e => e.ImageUrl)
            .HasMaxLength(300);

        entity.Property(e => e.ImageGalleryJson)
            .HasColumnType("text");

        entity.Property(e => e.CostPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        entity.Property(e => e.SellPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        entity.Property(e => e.StockQuantity)
            .HasDefaultValue(0);

        entity.Property(e => e.Description)
            .HasColumnType("text");

        entity.Property(e => e.IsDraft)
            .HasDefaultValue(false);

        entity.HasIndex(e => e.CategoryId);
        entity.HasIndex(e => e.ProductName);
        entity.HasIndex(e => e.BrandName);
    }

    private void ConfigureCategory(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Category>();

        entity.HasKey(e => e.CategoryId);

        entity.Property(e => e.CategoryName)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.Description)
            .HasMaxLength(255);

        entity.HasIndex(e => e.CategoryName)
            .IsUnique();
    }

    private void ConfigureOrder(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Order>();

        entity.HasKey(e => e.OrderId);

        entity.Property(e => e.UserId)
            .IsRequired();

        entity.Property(e => e.OrderDate)
            .IsRequired();

        entity.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        entity.Property(e => e.SubtotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        entity.Property(e => e.Discount)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        entity.Property(e => e.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        entity.Property(e => e.Notes)
            .HasMaxLength(1000)
            .HasColumnType("text");

        entity.HasIndex(e => e.CustomerId);
        entity.HasIndex(e => e.UserId);
        entity.HasIndex(e => e.OrderDate);
        entity.HasIndex(e => e.Status);
    }

    private void ConfigureOrderDetail(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<OrderDetail>();

        entity.HasKey(e => e.OrderDetailId);

        entity.Property(e => e.OrderId)
            .IsRequired();

        entity.Property(e => e.ProductId)
            .IsRequired();

        entity.Property(e => e.Quantity)
            .IsRequired();

        entity.Property(e => e.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // SubTotal is a computed property and should not be mapped
        entity.Ignore(e => e.SubTotal);

        entity.HasIndex(e => e.OrderId);
        entity.HasIndex(e => e.ProductId);
        
        // Composite index for Order-Product uniqueness (one product per order line)
        entity.HasIndex(e => new { e.OrderId, e.ProductId });
    }

    private void ConfigurePayment(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Payment>();

        entity.HasKey(e => e.PaymentId);

        entity.Property(e => e.OrderId)
            .IsRequired();

        entity.Property(e => e.PaymentMethod)
            .IsRequired()
            .HasConversion<int>();

        entity.Property(e => e.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        entity.Property(e => e.PaymentDate)
            .IsRequired();

        entity.HasIndex(e => e.OrderId);
        entity.HasIndex(e => e.PaymentDate);
    }

    private void ConfigureUser(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<User>();

        entity.HasKey(e => e.UserId);

        entity.Property(e => e.UserFullName)
            .IsRequired()
            .HasMaxLength(150);

        entity.Property(e => e.UserName)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);

        entity.Property(e => e.RoleId)
            .IsRequired();

        entity.Property(e => e.IsActive)
            .HasDefaultValue(true);

        entity.Property(e => e.HasSeenGuide)
            .HasDefaultValue(false);

        entity.HasIndex(e => e.UserName)
            .IsUnique();
        entity.HasIndex(e => e.RoleId);
    }

    private void ConfigureRole(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Role>();

        entity.HasKey(e => e.RoleId);

        entity.Property(e => e.RoleName)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.Description)
            .HasMaxLength(500);

        entity.HasIndex(e => e.RoleName)
            .IsUnique();
    }

    private void ConfigureCommission(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Commission>();

        entity.HasKey(e => e.CommissionId);

        entity.Property(e => e.UserId)
            .IsRequired();

        entity.Property(e => e.Month)
            .IsRequired();

        entity.Property(e => e.Year)
            .IsRequired();

        entity.Property(e => e.TotalSales)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        entity.Property(e => e.CommissionRate)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        entity.Property(e => e.Note)
            .HasMaxLength(255);

        entity.Property(e => e.CreatedAt)
            .IsRequired();

        // CommissionAmount is a computed property and should not be mapped
        entity.Ignore(e => e.CommissionAmount);

        entity.HasIndex(e => e.UserId);
        entity.HasIndex(e => new { e.UserId, e.Month, e.Year })
            .IsUnique();
        entity.HasIndex(e => new { e.Year, e.Month });
    }

    private void ConfigureAppSetting(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AppSetting>();

        entity.HasKey(e => e.AppSettingId);

        entity.Property(e => e.Key)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.Value)
            .HasMaxLength(1000);

        entity.Property(e => e.ValueType)
            .IsRequired()
            .HasConversion<int>();

        entity.Property(e => e.Category)
            .HasMaxLength(50);

        entity.Property(e => e.Description)
            .HasMaxLength(255);

        entity.Property(e => e.IsSystem)
            .IsRequired()
            .HasDefaultValue(false);

        entity.Property(e => e.UpdatedAt)
            .IsRequired();

        entity.HasIndex(e => e.Key)
            .IsUnique();
        entity.HasIndex(e => e.Category);
    }

    private void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        // Category -> Product (One-to-Many)
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of category with products

        // Customer -> Order (One-to-Many, optional)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders!)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.SetNull); // If customer is deleted, set CustomerId to null

        // User -> Order (One-to-Many)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders!)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of user with orders

        // Order -> OrderDetail (One-to-Many)
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails!)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete order details when order is deleted

        // Product -> OrderDetail (One-to-Many)
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Product)
            .WithMany(p => p.OrderDetails!)
            .HasForeignKey(od => od.ProductId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of product with order details

        // Order -> Payment (One-to-Many)
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithMany(o => o.Payments!)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete payments when order is deleted

        // Role -> User (One-to-Many)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users!)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of role with users

        // User -> Commission (One-to-Many)
        modelBuilder.Entity<Commission>()
            .HasOne(c => c.User)
            .WithMany(u => u.Commissions!)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete commissions when user is deleted
    }

    private void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Additional composite indexes for common query patterns

        // Orders by date range queries
        modelBuilder.Entity<Order>()
            .HasIndex(e => new { e.OrderDate, e.Status });

        // Products search optimization
        modelBuilder.Entity<Product>()
            .HasIndex(e => new { e.CategoryId, e.IsDraft });

        // Active users query
        modelBuilder.Entity<User>()
            .HasIndex(e => new { e.IsActive, e.RoleId });
    }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// This method will automatically call DetectChanges() before saving.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Asynchronously saves all changes made in this context to the database.
    /// This method will automatically call DetectChanges() before saving.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Automatically updates CreatedAt and UpdatedAt timestamps for entities.
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is not null && 
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            if (entry.Entity is Customer customer)
            {
                if (entry.State == EntityState.Added)
                {
                    customer.CreatedAt = DateTime.Now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    customer.UpdatedAt = DateTime.Now;
                }
            }
            else if (entry.Entity is Product product)
            {
                if (entry.State == EntityState.Added)
                {
                    product.CreatedAt = DateTime.Now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    product.UpdatedAt = DateTime.Now;
                }
            }
            else if (entry.Entity is AppSetting appSetting && entry.State == EntityState.Modified)
            {
                appSetting.UpdatedAt = DateTime.Now;
            }
            else if (entry.Entity is Commission commission && entry.State == EntityState.Added)
            {
                commission.CreatedAt = DateTime.Now;
            }
        }
    }
}

