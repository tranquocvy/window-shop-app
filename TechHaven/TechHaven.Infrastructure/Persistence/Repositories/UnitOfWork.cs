using Microsoft.EntityFrameworkCore.Storage;
using TechHaven.Domain.Interfaces;
using TechHaven.Infrastructure.Persistence.Repositories;

namespace TechHaven.Infrastructure.Persistence;

/// <summary>
/// Implementation of the Unit of Work pattern.
/// Coordinates multiple repositories and manages a single database transaction.
/// All repositories share the same DbContext instance, ensuring consistency.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    // Lazy-loaded repository instances
    private IUserRepository? _users;
    private IRoleRepository? _roles;
    private IProductRepository? _products;
    private ICustomerRepository? _customers;
    private IOrderRepository? _orders;
    private IPaymentRepository? _payments;
    private ICommissionRepository? _commissions;
    private IAppSettingRepository? _appSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="context">The database context to coordinate.</param>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    public UnitOfWork(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region Repository Properties (Lazy Initialization)

    /// <inheritdoc />
    public IUserRepository Users
    {
        get
        {
            if (_users == null)
            {
                _users = new UserRepository(_context);
            }
            return _users;
        }
    }

    /// <inheritdoc />
    public IRoleRepository Roles
    {
        get
        {
            if (_roles == null)
            {
                _roles = new RoleRepository(_context);
            }
            return _roles;
        }
    }

    /// <inheritdoc />
    public IProductRepository Products
    {
        get
        {
            if (_products == null)
            {
                _products = new ProductRepository(_context);
            }
            return _products;
        }
    }

    /// <inheritdoc />
    public ICustomerRepository Customers
    {
        get
        {
            if (_customers == null)
            {
                _customers = new CustomerRepository(_context);
            }
            return _customers;
        }
    }

    /// <inheritdoc />
    public IOrderRepository Orders
    {
        get
        {
            if (_orders == null)
            {
                _orders = new OrderRepository(_context);
            }
            return _orders;
        }
    }

    /// <inheritdoc />
    public IPaymentRepository Payments
    {
        get
        {
            if (_payments == null)
            {
                _payments = new PaymentRepository(_context);
            }
            return _payments;
        }
    }

    /// <inheritdoc />
    public ICommissionRepository Commissions
    {
        get
        {
            if (_commissions == null)
            {
                _commissions = new CommissionRepository(_context);
            }
            return _commissions;
        }
    }

    /// <inheritdoc />
    public IAppSettingRepository AppSettings
    {
        get
        {
            if (_appSettings == null)
            {
                _appSettings = new AppSettingRepository(_context);
            }
            return _appSettings;
        }
    }

    #endregion

    #region Save Operations

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            // Log the exception here if you have a logging framework
            // _logger.LogError(ex, "Error occurred while saving changes");
            throw; // Re-throw to let the caller handle it
        }
    }

    #endregion

    #region Explicit Transaction Control

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException(
                "A transaction is already in progress. " +
                "Please commit or rollback the current transaction before starting a new one.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException(
                "No transaction is in progress. " +
                "Call BeginTransactionAsync() before attempting to commit.");
        }

        try
        {
            // Commit the transaction
            await _transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            // If commit fails, rollback
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            // Dispose the transaction
            await DisposeTransactionAsync();
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException(
                "No transaction is in progress. " +
                "Call BeginTransactionAsync() before attempting to rollback.");
        }

        try
        {
            // Rollback the transaction
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            // Dispose the transaction
            await DisposeTransactionAsync();
        }
    }

    /// <summary>
    /// Disposes the current transaction if one exists.
    /// </summary>
    private async Task DisposeTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>
    /// Releases the managed and unmanaged resources used by the <see cref="UnitOfWork"/>.
    /// </summary>
    /// <param name="disposing">
    /// True to release both managed and unmanaged resources; 
    /// false to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                _transaction?.Dispose();
                _context.Dispose();
            }

            // Dispose unmanaged resources (if any)
            // None in this case

            _disposed = true;
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// When using Dependency Injection with Scoped lifetime,
    /// this method is automatically called at the end of each request.
    /// Manual disposal is only needed when creating instances directly (not recommended).
    /// </remarks>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer that ensures resources are released if Dispose is not called.
    /// </summary>
    ~UnitOfWork()
    {
        Dispose(disposing: false);
    }

    #endregion
}