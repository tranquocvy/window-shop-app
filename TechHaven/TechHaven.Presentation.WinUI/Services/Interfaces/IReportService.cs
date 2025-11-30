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
        Task<List<ProductSalesDto>> GetProductSalesReportAsync(ReportQueryDto query);

        /// <summary>
        /// Get revenue and profit report (for bar chart)
        /// </summary>
        Task<List<SalesReportDto>> GetRevenueReportAsync(ReportQueryDto query);

        /// <summary>
        /// Get list of products for dropdown
        /// </summary>
        Task<List<ProductSummaryDto>> GetProductsAsync();
    }

    public class ProductSummaryDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }
}
