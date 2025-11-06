using System;
using System.Collections.Generic;
using System.Threading;
using TechHaven.Domain.Entities;

namespace TechHaven.Domain.Interfaces
{
    public interface ICommissionRepository : IGenericRepository<Commission>
    {
        // Lấy hoa hồng theo user và tháng/năm
        Task<Commission?> GetByUserAndMonthAsync(
            int userId,
            int month,
            int year,
            CancellationToken cancellationToken = default);

        // Lấy hoa hồng theo user (KPI)
        Task<IReadOnlyList<Commission>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);

        // Lấy hoa hồng theo tháng/năm (báo cáo)
        Task<IReadOnlyList<Commission>> GetByMonthYearAsync(
            int month, 
            int year, 
            CancellationToken cancellationToken = default);

        // Tính tổng hoa hồng theo user trong khoảng thời gian
        Task<decimal> GetTotalCommissionAsync(
            int userId, 
            int startMonth, 
            int startYear, 
            int endMonth, 
            int endYear, 
            CancellationToken cancellationToken = default);
    }
}