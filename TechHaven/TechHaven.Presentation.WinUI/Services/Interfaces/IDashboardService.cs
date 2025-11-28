using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Dashboard;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface IDashboardService
    {
        /// <summary>
        /// Lấy dữ liệu tổng quan cho trang dashboard
        /// </summary>
        Task<ResponseWrapper<DashboardDto>> GetDashboardAsync();
    }
}
