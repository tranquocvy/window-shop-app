using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Reports;
using CommissionQueryDto = TechHaven.Shared.DTOs.Reports.CommissionQueryDto;

namespace TechHaven.Presentation.WinUI.Services.Http
{
    public class HttpReportService : IReportService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "/api/Report";

        public HttpReportService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<ProductSalesTrendDto>> GetProductSalesReportAsync(ReportQueryDto query)
        {
            try
            {
                var url = $"{BaseUrl}/product-sales?" +
                         $"startDate={query.StartDate:yyyy-MM-dd}&" +
                         $"endDate={query.EndDate:yyyy-MM-dd}&" +
                         $"periodType={query.PeriodType}";

                var response = await _httpClient.GetFromJsonAsync<List<ProductSalesTrendDto>>(url);
                return response ?? new List<ProductSalesTrendDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<ProductSalesTrendDto>();
            }
        }

        public async Task<SalesTrendDto> GetRevenueReportAsync(ReportQueryDto query)
        {
            try
            {
                var url = $"{BaseUrl}/trend?startDate={query.StartDate:yyyy-MM-dd}&endDate={query.EndDate:yyyy-MM-dd}&periodType={query.PeriodType}";
                var response = await _httpClient.GetFromJsonAsync<SalesTrendDto>(url);
                return response ?? new SalesTrendDto();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new SalesTrendDto();
            }
        }

        public async Task<List<ProductSummaryDto>> GetProductsAsync(string? keyword = null)
        {
            try
            {
                var url = "/api/Product";
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    url += $"?SearchTerm={Uri.EscapeDataString(keyword)}";
                }

                var response = await _httpClient.GetFromJsonAsync<List<ProductSummaryDto>>(url);
                return response ?? new List<ProductSummaryDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<ProductSummaryDto>();
            }
        }

        public async Task<List<CommissionReportDto>> GetCommissionReportAsync(CommissionQueryDto query)
        {
            try
            {
                var url = $"{BaseUrl}/commission?month={query.Month}&year={query.Year}";

                var response = await _httpClient.GetFromJsonAsync<List<CommissionReportDto>>(url);
                return response ?? new List<CommissionReportDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<CommissionReportDto>();
            }
        }
    }
}
