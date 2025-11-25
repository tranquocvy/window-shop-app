using Microsoft.EntityFrameworkCore;
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

        if (criteria.ToDate.HasValue) { 
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
}