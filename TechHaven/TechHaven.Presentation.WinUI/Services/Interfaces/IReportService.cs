using System.Collections.Generic;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Reports;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface IReportService
    {
        /// <summary>
        /// Get product sales report (for line chart)
        /// </summary>
        Task<List<ProductSalesTrendDto>> GetProductSalesReportAsync(ReportQueryDto query);

        /// <summary>
        /// Get revenue and profit report (trend with summary)
        /// </summary>
        Task<SalesTrendDto> GetRevenueReportAsync(ReportQueryDto query);

        /// <summary>
        /// Get list of products for dropdown or search
        /// </summary>
        Task<List<ProductSummaryDto>> GetProductsAsync(string? keyword = null);

        /// <summary>
        /// Get commission report grouped by user (admin only)
        /// </summary>
        Task<List<CommissionReportDto>> GetCommissionReportAsync(ReportQueryDto query);
    }

    public class ProductSummaryDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
    }
}
