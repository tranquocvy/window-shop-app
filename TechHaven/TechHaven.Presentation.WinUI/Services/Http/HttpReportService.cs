using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Reports;
using TechHaven.Shared.DTOs.Common;
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

        private async Task<T?> GetWrappedAsync<T>(string url) where T : class
        {
            try
            {
                var wrapper = await _httpClient.GetFromJsonAsync<ResponseWrapper<T>>(url);
                return wrapper?.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HttpReportService.GetWrappedAsync error: {ex.Message}");
                return default;
            }
        }

        public async Task<List<ProductSalesTrendDto>> GetProductSalesReportAsync(ReportQueryDto query)
        {
            try
            {
                var url = $"{BaseUrl}/product-sales?" +
                         $"startDate={query.StartDate:yyyy-MM-dd}&" +
                         $"endDate={query.EndDate:yyyy-MM-dd}&" +
                         $"periodType={query.PeriodType}";

                var data = await GetWrappedAsync<List<ProductSalesTrendDto>>(url);
                return data ?? new List<ProductSalesTrendDto>();
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
                var url = $"{BaseUrl}/sales?StartDate={query.StartDate:yyyy-MM-dd}&EndDate={query.EndDate:yyyy-MM-dd}&PeriodType={query.PeriodType}";
                var data = await GetWrappedAsync<SalesTrendDto>(url);
                return data ?? new SalesTrendDto();
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

                var data = await GetWrappedAsync<List<ProductSummaryDto>>(url);
                return data ?? new List<ProductSummaryDto>();
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

                var data = await GetWrappedAsync<List<CommissionReportDto>>(url);
                return data ?? new List<CommissionReportDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<CommissionReportDto>();
            }
        }

        public async Task<ProductSalesTrendDto> GetProductDetailAsync(int productId, ReportQueryDto query)
        {
            try
            {
                var url = $"{BaseUrl}/products/{productId}?StartDate={query.StartDate:yyyy-MM-dd}&EndDate={query.EndDate:yyyy-MM-dd}&PeriodType={(int)query.PeriodType}";
                var data = await GetWrappedAsync<ProductSalesTrendDto>(url);
                return data ?? new ProductSalesTrendDto();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new ProductSalesTrendDto();
            }
        }
    }
}
