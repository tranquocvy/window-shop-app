using System;
using System.Collections.Generic;
using System.Threading;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Enums;
using TechHaven.Domain.SearchCriteria;

namespace TechHaven.Domain.Interfaces
{

    /// <summary>
    /// Order-specific repository supporting IQueryable for
    /// server-side filtering, sorting, and paging.
    /// </summary>
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<(IReadOnlyList<Order> Items, int TotalCount)>
        SearchOrdersAsync(
            OrderSearchCriteria criteria,
            CancellationToken cancellationToken = default
        );

        // Get detailed order info.
        Task<Order?> GetWithDetailsAsync(int orderId, CancellationToken cancellationToken = default);

        // ============================================
        // Interface for Dashboard
        // ============================================

        /// <summary>
        /// Lấy tổng số đơn hàng trong ngày
        /// </summary>
        Task<int> GetTodayOrderCountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy tổng doanh thu trong ngày
        /// </summary>
        Task<decimal> GetTodayRevenueAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy N đơn hàng gần nhất
        /// </summary>
        Task<IReadOnlyList<Order>> GetRecentOrdersAsync(
            int count = 3,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Lấy doanh thu theo ngày trong tháng
        /// </summary>
        Task<Dictionary<
            DateTime,
            (decimal Revenue, int OrderCount)
        >> GetMonthlyRevenueAsync(
            int year,
            int month,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Lấy top N sản phẩm bán chạy nhất
        /// </summary>
        Task<List<(
            int ProductId,
            string ProductName,
            string BrandName,
            int TotalSold,
            decimal TotalRevenue
        )>> GetTopSellingProductsAsync(
            int count = 5,
            CancellationToken cancellationToken = default
        );

        // ============================================
        // Interface for Report
        // ============================================

        /// <summary>
        /// Lấy báo cáo doanh số theo khoảng thời gian
        /// </summary>
        Task<List<(
            string Period,
            int TotalOrders,
            decimal TotalRevenue,
            decimal TotalCost,
            decimal Profit,
            decimal ProfitMargin
        )>> GetSalesReportAsync(
            DateTime startDate,
            DateTime endDate,
            ReportPeriodType periodType,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Lấy top N sản phẩm bán chạy nhất trong khoảng thời gian
        /// </summary>
        Task<List<(
            int ProductId,
            string ProductName,
            string BrandName,
            int TotalSold,
            decimal TotalRevenue
        )>> GetTopSellingProductsAsync(
            DateTime startDate,
            DateTime endDate,
            int count = 10,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Lấy báo cáo hoa hồng nhân viên
        /// </summary>
        Task<List<(
            int UserId,
            string UserFullName,
            string RoleName,
            decimal TotalSales,
            decimal CommissionRate,
            decimal CommissionAmount,
            int TotalOrders
        )>> GetCommissionReportAsync(
            DateTime startDate,
            DateTime endDate,
            int? userId = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Lấy tất cả đơn hàng trong ngày
        /// </summary>
        Task<IReadOnlyList<Order>> GetTodayOrders(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Lấy tất cả đơn hàng trong tháng
        /// </summary>
        Task<IReadOnlyList<Order>> GetMonthOrders(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Lấy số lượng sản phẩm theo Status
        /// </summary>
        Task<Dictionary<OrderStatus, int>> GetOrderStatusCountsAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default
        );
    }
}