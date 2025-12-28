using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Entities;
using TechHaven.Infrastructure.Services;

namespace TechHaven.Infrastructure.Persistence;

/// <summary>
/// Represents the database context for the TechHaven application using Entity Framework Core.
/// </summary>
public class AppDbContext : DbContext
{
    private readonly ITenantService? _tenantService;
    private readonly IConfiguration? _configuration;
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by a DbContext.</param>
    //Constructor 2: Dùng cho khởi tạo thủ công (Login/Verify External Handler)
    // Flow này bạn tự new optionsBuilder nên không cần tenantService
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Constructor 1: Dùng cho DI (Flow chính + Middleware)
    // Middleware sẽ set dữ liệu vào tenantService, và nó được inject vào đây
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ITenantService tenantService,
        IConfiguration configuration
        ) : base(options)
    {
        _tenantService = tenantService;
        _configuration = configuration;
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

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // 1. Kiểm tra xem TenantService có chuỗi kết nối External không?
        // (Đây là chuỗi do TenantMiddleware giải mã từ Token và nạp vào)
        if (_tenantService != null && !string.IsNullOrEmpty(_tenantService.ConnectionString))
        {
            // [QUAN TRỌNG] Nếu có External DB, ta GHI ĐÈ cấu hình mặc định.
            optionsBuilder.UseNpgsql(_tenantService.ConnectionString, npgsqlOptions =>
            {
                // Copy lại các cấu hình phụ trợ để đảm bảo đồng bộ behavior
                npgsqlOptions.MigrationsAssembly("TechHaven.Infrastructure");
                npgsqlOptions.EnableRetryOnFailure(3);
                npgsqlOptions.CommandTimeout(30);
            })
            .UseSnakeCaseNamingConvention(); // Đảm bảo naming convention giống nhau

            // Log an toàn hơn (chỉ log là đã switch, không log password)
             System.Diagnostics.Debug.WriteLine("[AppDbContext] Switched to External Tenant DB");
        }
        // 2. Nếu KHÔNG có External DB (_tenantService rỗng hoặc null)
        // THÌ KHÔNG LÀM GÌ CẢ!
        // Vì DependencyInjection.cs đã cấu hình Default Connection rồi.
        // Nếu ta viết thêm code cấu hình default ở đây, nó sẽ bị trùng lặp và thừa thãi.
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
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Product product)
            {
                if (product.CreatedAt.Kind != DateTimeKind.Utc)
                {
                    product.CreatedAt = DateTime.SpecifyKind(product.CreatedAt, DateTimeKind.Utc);
                }
            }
        }

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
                appSetting.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Commission commission && entry.State == EntityState.Added)
            {
                commission.CreatedAt = DateTime.Now;
            }
        }
    }
}