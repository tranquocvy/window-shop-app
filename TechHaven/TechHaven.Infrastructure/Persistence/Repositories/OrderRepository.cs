using Microsoft.EntityFrameworkCore;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Order entity with detailed queries.
/// </summary>
public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetWithDetailsAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.User)
                .ThenInclude(u => u!.Role)
            .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.User)
                .ThenInclude(u => u!.Role)
            .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.User)
            .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByStatusAsync(int status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.User)
            .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
            .Where(o => (int)o.Status == status)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.User)
            .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Order?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await GetWithDetailsAsync((int)id, cancellationToken);
    }
}