using System;
using System.Collections.Generic;
using System.Threading;
using TechHaven.Domain.Entities;

namespace TechHaven.Domain.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        // Lấy order kèm OrderDetail, Payment, User, Customer (Transaction data)
        Task<Order?> GetWithDetailsAsync(int orderId, CancellationToken cancellationToken = default);

        // Lấy danh sách order kèm chi tiết
        Task<IReadOnlyList<Order>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);

        // Lấy order theo customer
        Task<IReadOnlyList<Order>> GetByCustomerAsync(int customerId, CancellationToken cancellationToken = default);

        // Lấy order theo user (sale)
        Task<IReadOnlyList<Order>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);

        // Lấy order theo trạng thái (pending, completed, cancelled)
        Task<IReadOnlyList<Order>> GetByStatusAsync(int status, CancellationToken cancellationToken = default);

        // Lấy order trong khoảng ngày (cho báo cáo)
        Task<IReadOnlyList<Order>> GetByDateRangeAsync(
            DateTime startDate, 
            DateTime endDate, 
            CancellationToken cancellationToken = default);
    }
}