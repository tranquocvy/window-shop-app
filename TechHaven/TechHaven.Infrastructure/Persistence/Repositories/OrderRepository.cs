using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Math.EC.Rfc7748;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Enums;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.SearchCriteria;
using TechHaven.Domain.Specifications;
using TechHaven.Shared.DTOs.Dashboard;
using TechHaven.Shared.DTOs.Users;

namespace TechHaven.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Order entity with detailed queries.
/// </summary>
public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context, ILoggerFactory loggerFactory)
        : base(context, loggerFactory)
    {
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)>
    SearchOrdersAsync(OrderSearchCriteria criteria, CancellationToken cancellationToken)
    {
        return await ExecuteOperationAsync(
            "SearchOrders",
            async () =>
            {
                var query = _context.Orders.AsQueryable();

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

                var totalCount = await query.CountAsync(cancellationToken);

                query = criteria.SortDescending
                        ? query.OrderByDescending(o => o.OrderDate)
                        : query.OrderBy(o => o.OrderDate);

                var items = await query
                    .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                    .Take(criteria.PageSize)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation(
                    "Order search returned {Count}/{Total} rows for criteria {@Criteria}",
                    items.Count,
                    totalCount,
                    criteria);

                return ((IReadOnlyList<Order>)items, totalCount);
            },
            new
            {
                criteria.PageNumber,
                criteria.PageSize,
                criteria.CustomerId,
                criteria.UserId,
                criteria.Status
            });
    }

    //TODO: Cần kiểm tra _dbSet có đúng là _context.Orders không?
    public async Task<Order?> GetWithDetailsAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetWithDetails",
            () => _dbSet
                .Include(o => o.Customer)
                .Include(o => o.User)
                    .ThenInclude(u => u!.Role)
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken),
            new { orderId });
    }

    // IMPLEMENT REPOSITORY FOR DASHBOARD

    public async Task<int> GetTodayOrderCountAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await ExecuteOperationAsync(
            "GetTodayOrderCount",
            () => _dbSet
                .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow)
                .CountAsync(cancellationToken));
    }

    public async Task<decimal> GetTodayRevenueAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await ExecuteOperationAsync(
            "GetTodayRevenue",
            () => _dbSet
                .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow)
                .Where(o => o.Status == Domain.Enums.OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount, cancellationToken));
    }

    public async Task<IReadOnlyList<Order>> GetRecentOrdersAsync(int count = 3, CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetRecentOrders",
            () => _dbSet
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync(cancellationToken),
            new { count });
    }

    public async Task<Dictionary<DateTime, (decimal Revenue, int OrderCount)>> GetMonthlyRevenueAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var orders = await ExecuteOperationAsync(
            "GetMonthlyRevenueRaw",
            () => _dbSet
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
                .Where(o => o.Status == OrderStatus.Completed)
                .Select(o => new
                {
                    Date = o.OrderDate.Date,
                    o.TotalAmount
                })
                .ToListAsync(cancellationToken),
            new { year, month });

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

    public async Task<List<(int ProductId, string ProductName, string BrandName, int TotalSold, decimal TotalRevenue)>>
    GetTopSellingProductsAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        // Lấy cả order và order detail để tính TotalSold và TotalRevenue
        var productSales = await ExecuteOperationAsync(
            "GetTopSellingProductsRaw",
            () => _context.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                .Where(od => od.Order!.Status == OrderStatus.Completed)
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
                    TotalRevenue = g.Sum(od => od.Quantity * od.UnitPrice)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(count)
                .ToListAsync(cancellationToken),
            new { count });

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

    // Todo: Đang bị lặp code với thằng trên => refactor lại cho đỡ lặp code
    public async Task<List<(int ProductId, string ProductName, string BrandName, int TotalSold, decimal TotalRevenue)>>
    GetTopSellingProductsAsync(
        DateTime startDate,
        DateTime endDate,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        // Ensure dates are in UTC
        var startUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(endDate.Date.AddDays(1), DateTimeKind.Utc);

        var productSales = await ExecuteOperationAsync(
            "GetTopSellingProductsRaw",
            () => _context.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                .Where(od => od.Order!.OrderDate >= startUtc && od.Order.OrderDate < endUtc)
                .Where(od => od.Order!.Status == OrderStatus.Completed)
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
                    TotalRevenue = g.Sum(od => od.Quantity * od.UnitPrice)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(count)
                .ToListAsync(cancellationToken),
            new { count });

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

    // Lấy báo cáo doanh số theo khoảng thời gian
    public async Task<List<(string Period, int TotalOrders, decimal TotalRevenue, decimal TotalCost, decimal Profit, decimal ProfitMargin)>>
    GetSalesReportAsync(DateTime startDate, DateTime endDate, ReportPeriodType periodType, CancellationToken cancellationToken = default)
    {
        // Ensure dates are in UTC
        var startUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(endDate.Date.AddDays(1), DateTimeKind.Utc);

        var orderQuery = _context.Orders
            .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
            .Where(o => o.OrderDate >= startUtc && o.OrderDate < endUtc)
            .Where(o => o.Status == OrderStatus.Completed);

        var orders = await orderQuery.ToListAsync(cancellationToken);

        var groupedData = periodType switch
        {
            ReportPeriodType.Daily => GroupByDay(orders),
            ReportPeriodType.Monthly => GroupByMonth(orders),
            ReportPeriodType.Weekly => GroupByWeek(orders),
            ReportPeriodType.Yearly => GroupByYear(orders),
            _ => GroupByDay(orders)
        };

        return groupedData;
    }

    public async Task<List<(string Period, int QuantitySold, decimal Revenue)>> GetProductSalesDataPointAsync
    (
        int productId,
        DateTime startDate,
        DateTime endDate,
        ReportPeriodType periodType,
        CancellationToken cancellationToken = default
    )
    {
        var spec = new OrdersByProductAndDateRangeSpecification(productId, startDate, endDate);

        _logger.LogInformation
        (
            "Searching orders by product id {@ProductId} and date range from {@StartDate} to {@EndDate}",
            productId, startDate, endDate
        );

        var items = await GetAsync(spec, cancellationToken);
            
        var groupedData = periodType switch
        {
            ReportPeriodType.Daily => items
                .GroupBy(o => o.OrderDate)
                .Select(g => (
                    Period: g.Key.ToString("yyyy-MM-dd"),
                    QuantitySold: g.SelectMany(o => o.OrderDetails!).Sum(od => od.Quantity),
                    Revenue: g.SelectMany(o => o.OrderDetails!).Sum(od => od.Quantity * od.UnitPrice)
                )),
            ReportPeriodType.Monthly => items
                .GroupBy(o => new
                {
                    o.OrderDate.Year,
                    o.OrderDate.Month,
                })
                .Select(g => (
                    Period: $"{g.Key.Year}-W{g.Key.Month:D2}",
                    QuantitySold: g.SelectMany(o => o.OrderDetails!).Sum(od => od.Quantity),
                    Revenue: g.SelectMany(o => o.OrderDetails!).Sum(od => od.Quantity * od.UnitPrice)
                )),
            ReportPeriodType.Weekly => items
                .GroupBy(o => new
                {
                    Year = o.OrderDate.Year,
                    Week = GetWeekOfYear(o.OrderDate),
                })
                .Select(g => (
                    Period: $"{g.Key.Year}-W{g.Key.Week:D2}",
                    QuantitySold: g.SelectMany(o => o.OrderDetails!).Sum(od => od.Quantity),
                    Revenue: g.SelectMany(o => o.OrderDetails!).Sum(od => od.Quantity * od.UnitPrice)
                )),
            ReportPeriodType.Yearly => items
                .GroupBy(o => o.OrderDate)
                .Select(g => (
                    Period: g.Key.ToString("yyyy-MM-dd"),
                    QuantitySold: g.SelectMany(o => o.OrderDetails!).Sum(od => od.Quantity),
                    Revenue: g.SelectMany(o => o.OrderDetails!).Sum(od => od.Quantity * od.UnitPrice)
                )),
            _ => items
                .GroupBy(o => o.OrderDate)
                .Select(g => (
                    Period: g.Key.ToString("yyyy-MM-dd"),
                    QuantitySold: g.SelectMany(o => o.OrderDetails!).Sum(od => od.Quantity),
                    Revenue: g.SelectMany(o => o.OrderDetails!).Sum(od => od.Quantity * od.UnitPrice)
                )),
        };

        return groupedData.ToList();
    }

    private List<(string Period, int TotalOrders, decimal TotalRevenue, decimal TotalCost, decimal Profit, decimal ProfitMargin)>
    GroupByDay(List<Order> orders)
    {
        return orders
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => (
                Period: g.Key.ToString("yyyy-MM-dd"),
                TotalOrders: g.Count(),
                TotalRevenue: g.Sum(o => o.TotalAmount),
                TotalCost: CalculateTotalCost(g.SelectMany(o => o.OrderDetails!).ToList()),
                Profit: g.Sum(o => o.TotalAmount) - CalculateTotalCost(g.SelectMany(o => o.OrderDetails!).ToList()),
                ProfitMargin: CalculateProfitMargin(
                    g.Sum(o => o.TotalAmount),
                    CalculateTotalCost(g.SelectMany(o => o.OrderDetails!).ToList())
                )
            ))
            .OrderBy(r => r.Period)
            .ToList();
    }

    private List<(string Period, int TotalOrders, decimal TotalRevenue, decimal TotalCost, decimal Profit, decimal ProfitMargin)>
    GroupByWeek(List<Order> orders)
    {
        return orders
            .GroupBy(o => new
            {
                Year = o.OrderDate.Year,
                Week = GetWeekOfYear(o.OrderDate)
            })
            .Select(g =>
            (
                Period: $"{g.Key.Year}-W{g.Key.Week:D2}",
                TotalOrders: g.Count(),
                TotalRevenue: g.Sum(o => o.TotalAmount),
                TotalCost: CalculateTotalCost(g.SelectMany(o => o.OrderDetails!).ToList()),
                Profit: g.Sum(o => o.TotalAmount) - CalculateTotalCost(g.SelectMany(o => o.OrderDetails!).ToList()),
                ProfitMargin: CalculateProfitMargin(
                    g.Sum(o => o.TotalAmount),
                    CalculateTotalCost(g.SelectMany(o => o.OrderDetails!).ToList())
                )
            ))
            .OrderBy(r => r.Period)
            .ToList();
    }

    private List<(string Period, int TotalOrders, decimal TotalRevenue, decimal TotalCost, decimal Profit, decimal ProfitMargin)>
    GroupByMonth(List<Order> orders)
    {
        return orders
            .GroupBy(o => new
            {
                o.OrderDate.Year,
                o.OrderDate.Month
            })
            .Select(g => (
                Period: $"{g.Key.Year}-{g.Key.Month:D2}",
                TotalOrders: g.Count(),
                TotalRevenue: g.Sum(o => o.TotalAmount),
                TotalCost: CalculateTotalCost(g.SelectMany(o => o.OrderDetails!).ToList()),
                Profit: g.Sum(o => o.TotalAmount) - CalculateTotalCost(g.SelectMany(o => o.OrderDetails!).ToList()),
                ProfitMargin: CalculateProfitMargin(
                    g.Sum(o => o.TotalAmount),
                    CalculateTotalCost(g.SelectMany(o => o.OrderDetails!).ToList())
                )
            ))
            .OrderBy(r => r.Period)
            .ToList();
    }

    private List<(string Period, int TotalOrders, decimal TotalRevenue, decimal TotalCost, decimal Profit, decimal ProfitMargin)>
    GroupByYear(List<Order> orders)
    {
        return orders
            .GroupBy(o => o.OrderDate.Year)
             .Select(g => (
                Period: g.Key.ToString(),
                TotalOrders: g.Count(),
                TotalRevenue: g.Sum(o => o.TotalAmount),
                TotalCost: CalculateTotalCost(g.SelectMany(o => o.OrderDetails!).ToList()),
                Profit: g.Sum(o => o.TotalAmount) - CalculateTotalCost(g.SelectMany(o => o.OrderDetails!).ToList()),
                ProfitMargin: CalculateProfitMargin(
                    g.Sum(o => o.TotalAmount),
                    CalculateTotalCost(g.SelectMany(o => o.OrderDetails!).ToList())
                )
            ))
            .OrderBy(r => r.Period)
            .ToList();
    }

    public async Task<List<(int UserId, string UserFullName, string RoleName, decimal TotalSales, decimal CommissionRate, decimal CommissionAmount, int TotalOrders)>>
    GetCommissionReportAsync(DateTime startDate, DateTime endDate, int? userId = null, CancellationToken cancellationToken = default)
    {
        // Ensure dates are in UTC
        var startUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(endDate.Date.AddDays(1), DateTimeKind.Utc);

        var query = _context.Orders
            .Include(o => o.User)
                .ThenInclude(u => u!.Role)
            .Where(o => o.OrderDate >= startUtc && o.OrderDate < endUtc)
            .Where(o => o.Status == OrderStatus.Completed)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        var data = await query
            .GroupBy(o => new
            {
                o.UserId,
                o.User!.UserFullName,
                RoleName = o.User.Role!.RoleName
            })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.UserFullName,
                g.Key.RoleName,
                TotalSales = g.Sum(o => o.TotalAmount),
                CommissionRate = 5.0m,
                CommissionAmount = g.Sum(o => o.TotalAmount) * 0.05m,
                TotalOrders = g.Count()
            })
            .OrderByDescending(x => x.TotalSales)
            .ToListAsync(cancellationToken);

        return data
            .Select(x => (
                x.UserId,
                x.UserFullName,
                x.RoleName,
                x.TotalSales,
                x.CommissionRate,
                x.CommissionAmount,
                x.TotalOrders
            ))
            .ToList();
    }

    public async Task<IReadOnlyList<Order>> GetTodayOrders(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        // Ensure UTC kind
        var todayUtc = DateTime.SpecifyKind(today, DateTimeKind.Utc);
        var tomorrowUtc = DateTime.SpecifyKind(tomorrow, DateTimeKind.Utc);

        return await _context.Orders
            .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
            .Where(o => o.OrderDate >= todayUtc && o.OrderDate < tomorrowUtc)
            .Where(o => o.Status == OrderStatus.Completed)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetMonthOrders(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfNextMonth = startOfMonth.AddMonths(1);

        return await _context.Orders
            .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
            .Where(o => o.OrderDate >= startOfMonth && o.OrderDate < startOfNextMonth)
            .Where(o => o.Status == OrderStatus.Completed)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<OrderStatus, int>> GetOrderStatusCountsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    )
    {
        // Ensure dates are in UTC
        var startUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(endDate.Date.AddDays(1), DateTimeKind.Utc);

        var orders = await ExecuteOperationAsync(
            "GetOrderStatusCountsAsync",
            () => _dbSet
                .Where(o => o.OrderDate >= startUtc && o.OrderDate < endUtc)
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                }).ToListAsync(cancellationToken),
            new { startDate, endDate }
        );

        // Ensure all statuses are represented
        var allStatuses = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>();
        var result = allStatuses.ToDictionary(status => status, status => 0);

        foreach (var order in orders)
        {
            result[order.Status] = order.Count;
        }

        return result;
    }

    // ============================================
    // HELPER METHODS
    // ============================================
    private decimal CalculateTotalCost(List<OrderDetail> orderDetails)
    {
        return orderDetails.Sum(od => od.Quantity * (od.Product?.CostPrice ?? 0));
    }

    private decimal CalculateProfitMargin(decimal revenue, decimal cost)
    {
        if (revenue == 0) return 0;
        return Math.Round((revenue - cost) / revenue * 100, 2);
    }

    private int GetWeekOfYear(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        return culture.Calendar.GetWeekOfYear(
            date,
            System.Globalization.CalendarWeekRule.FirstDay,
            DayOfWeek.Monday
        );
    }
}