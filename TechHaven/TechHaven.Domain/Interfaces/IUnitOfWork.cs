namespace TechHaven.Domain.Interfaces;

/// <summary>
/// Represents a Unit of Work pattern that coordinates the work of multiple repositories
/// by creating a single database context/transaction shared by all of them.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repository properties - Lazy initialization
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IProductRepository Products { get; }
    ICustomerRepository Customers { get; }
    IOrderRepository Orders { get; }
    IPaymentRepository Payments { get; }
    ICommissionRepository Commissions { get; }
    IAppSettingRepository AppSettings { get; }

    /// <summary>
    /// Saves all pending changes to the database in a single transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// Useful for explicit transaction control when needed.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}