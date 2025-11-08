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

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
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