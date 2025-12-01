using Microsoft.Extensions.Logging;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.SearchCriteria;
using TechHaven.Domain.Specifications;
using TechHaven.Infrastructure.Persistence;
using Org.BouncyCastle.Math.EC.Rfc7748;
using Microsoft.EntityFrameworkCore;



namespace TechHaven.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Order entity with detailed queries.
/// </summary>
public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
    {
    }
    // 1. Hàm Search cho danh sách 
    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> SearchWithPaginationAsync(
        OrderSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        var spec = new OrderSearchSpecification(criteria);
        _logger.LogInformation(
           "Searching orders with criteria {@Criteria}",
           criteria);

        var items = await GetAsync(spec, cancellationToken);

        // Count
        var countSpec = new OrderSearchSpecification(criteria, true); //for counting = true 
        var totalCount = await CountAsync(countSpec, cancellationToken);

        return (items, totalCount);
    }

    public async Task<Order?> GetWithDetailsAsync(int orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching order detail with order id {orderId}", orderId);
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.User)
                .ThenInclude(u => u!.Role)
            .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
    }  
}