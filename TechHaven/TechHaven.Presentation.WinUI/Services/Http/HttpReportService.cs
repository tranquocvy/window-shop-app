using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Reports;

namespace TechHaven.Presentation.WinUI.Services.Http
{
    public class HttpReportService : IReportService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "/api/reports";

        public HttpReportService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<ProductSalesDto>> GetProductSalesReportAsync(ReportQueryDto query)
        {
            try
            {
                var url = $"{BaseUrl}/product-sales?" +
                         $"startDate={query.StartDate:yyyy-MM-dd}&" +
                         $"endDate={query.EndDate:yyyy-MM-dd}&" +
                         $"periodType={query.PeriodType}";

                if (query.UserId.HasValue)
                {
                    url += $"&userId={query.UserId.Value}";
                }

                var response = await _httpClient.GetFromJsonAsync<List<ProductSalesDto>>(url);
                return response ?? new List<ProductSalesDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<ProductSalesDto>();
            }
        }

        public async Task<List<SalesReportDto>> GetRevenueReportAsync(ReportQueryDto query)
        {
            try
            {
                var url = $"{BaseUrl}/sales?" +
                         $"startDate={query.StartDate:yyyy-MM-dd}&" +
                         $"endDate={query.EndDate:yyyy-MM-dd}&" +
                         $"periodType={query.PeriodType}";

                var response = await _httpClient.GetFromJsonAsync<List<SalesReportDto>>(url);
                return response ?? new List<SalesReportDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<SalesReportDto>();
            }
        }

        public async Task<List<ProductSummaryDto>> GetProductsAsync()
        {
            try
            {
                var url = "/api/products/summary";
                var response = await _httpClient.GetFromJsonAsync<List<ProductSummaryDto>>(url);
                return response ?? new List<ProductSummaryDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<ProductSummaryDto>();
            }
        }
    }
}
