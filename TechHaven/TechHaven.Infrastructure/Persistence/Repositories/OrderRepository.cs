using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Math.EC.Rfc7748;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.SearchCriteria;

namespace TechHaven.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Order entity with detailed queries.
/// </summary>
public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)>
    SearchOrdersAsync(OrderSearchCriteria criteria, CancellationToken cancellationToken)
    {
        // 1. Khởi tạo Query Chưa chạy xuống DB)
        var query = _context.Orders.AsQueryable();

        // 2. Eager Loading (CẨN TRỌNG: Xem cảnh báo bên dưới)
        // Nếu chỉ hiển thị danh sách, bạn có thực sự cần load hết OrderDetails không?
        // Nếu cần, hãy Include. Nếu không, hãy bỏ qua để tối ưu.
        // Không cần load thông tin khách hàng
        //Bỏ: query = query.Include(o => o.User).Include(o => o.Customer);

        // 3. Áp dụng bộ lọc động (Dynamic Filtering)
        if (criteria.CustomerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == criteria.CustomerId);
        }

        if (criteria.UserId.HasValue)
        {
            query = query.Where(o => o.UserId == criteria.UserId);
        }

        if (criteria.Status.HasValue)
        {
            //TODO: Xem xét nếu enum OrderStatus thay đổi, cần đồng bộ chỗ này
            //Hot fix: Ép kiểu tạm int cho cả 2 biến để so sánh.
            query = query.Where(o => (int)o.Status == (int)criteria.Status.Value);
        }

        if (criteria.FromDate.HasValue)
        {
            query = query.Where(o => o.OrderDate >= criteria.FromDate.Value);
        }

        if (criteria.ToDate.HasValue)
        {
            query = query.Where(o => o.OrderDate <= criteria.ToDate.Value);
        }

        // 4. Đếm tổng số bản ghi (trước khi phân trang)
        // Đây là bước tốn kém nhưng cần thiết cho UI phân trang
        var totalCount = await query.CountAsync(cancellationToken);

        // 5. Sắp xếp (Sorting)
        // (Giả sử bạn có logic switch case hoặc thư viện DynamicLinq)
        query = criteria.SortDescending
                ? query.OrderByDescending(o => o.OrderDate)
                : query.OrderBy(o => o.OrderDate);

        // 6. Phân trang (Pagination)
        var items = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    //TODO: Cần kiểm tra _dbSet có đúng là _context.Orders không?
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

    // IMPLEMENT REPOSITORY FOR DASHBOARD

    public async Task<int> GetTodayOrderCountAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return await _dbSet
            .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow)
            .CountAsync(cancellationToken);
    }

    public async Task<decimal> GetTodayRevenueAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return await _dbSet
            .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow)
            .Where(o => o.Status == Domain.Enums.OrderStatus.Completed)
            .SumAsync(o => o.TotalAmount, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetRecentOrdersAsync(int count = 3, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<DateTime, (decimal Revenue, int OrderCount)>> GetMonthlyRevenueAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var orders = await _dbSet
            .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
            .Where(o => o.Status == Domain.Enums.OrderStatus.Completed)
            .Select(o => new
            {
                Date = o.OrderDate.Date,
                o.TotalAmount
            })
            .ToListAsync(cancellationToken);

        return orders
            .GroupBy(o => o.Date)
            .ToDictionary(
                g => g.Key,
                g => (
                    Revenue: g.Sum(x => x.TotalAmount),
                    OrderCount: g.Count()
                )
            );
    }

    public async Task<List<(int ProductId, string ProductName, string BrandName, int TotalSold, decimal TotalRevenue)>> GetTopSellingProductsAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        // Lấy cả order và order detail để tính TotalSold và TotalRevenue
        var productSales = await _context.OrderDetails
            .Include(od => od.Product)
            .Include(od => od.Order)
            .Where(od => od.Order!.Status == Domain.Enums.OrderStatus.Completed)
            .GroupBy(od => new
            {
                od.ProductId,
                od.Product!.ProductName,
                od.Product!.BrandName
            })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.BrandName,
                TotalSold = g.Sum(od => od.Quantity),
                TotalRevenue = g.Sum(od => od.SubTotal)
            })
            .OrderByDescending(x => x.TotalSold)
            .Take(count)
            .ToListAsync(cancellationToken);

        return productSales
            .Select(x => (
                x.ProductId,
                x.ProductName,
                x.BrandName,
                x.TotalSold,
                x.TotalRevenue
            ))
            .ToList();
    }
}