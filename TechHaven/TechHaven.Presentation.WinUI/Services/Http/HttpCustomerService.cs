using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Customers;
using System.Diagnostics;

namespace TechHaven.Presentation.WinUI.Services.Http
{
    public class HttpCustomerService : ICustomerService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/Customer";

        public HttpCustomerService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<ResponseWrapper<PagingResponse<CustomerDto>>> QueryCustomersAsync(CustomerListQueryDto query)
        {
            // Build query string from CustomerListQueryDto
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                queryParams.Add($"SearchTerm={Uri.EscapeDataString(query.SearchTerm)}");

            if (query.Type.HasValue)
                queryParams.Add($"Type={query.Type.Value}");

            if (query.CreatedAt?.StartDate.HasValue == true)
                queryParams.Add($"CreatedAt.StartDate={query.CreatedAt.StartDate.Value:O}");

            if (query.CreatedAt?.EndDate.HasValue == true)
                queryParams.Add($"CreatedAt.EndDate={query.CreatedAt.EndDate.Value:O}");

            if (query.Sorting != null)
            {
                if (!string.IsNullOrWhiteSpace(query.Sorting.SortBy))
                    queryParams.Add($"Sorting.SortBy={Uri.EscapeDataString(query.Sorting.SortBy)}");

                queryParams.Add($"Sorting.Desc={query.Sorting.Desc}");
            }

            if (query.PageNumber > 0)
                queryParams.Add($"PageNumber={query.PageNumber}");

            // Always include PageSize parameter when provided (>0)
            if (query.PageSize > 0)
                queryParams.Add($"PageSize={query.PageSize}");

            var queryString = string.Join("&", queryParams);
            var url = string.IsNullOrEmpty(queryString) ? BaseUrl : $"{BaseUrl}?{queryString}";

            // Debug log the final URL so we can confirm PageSize
            Debug.WriteLine($"HttpCustomerService.QueryCustomersAsync -> Request URL: {_httpClient.BaseAddress?.ToString().TrimEnd('/')}/{url}");

            return await _httpClient.GetWrapperFromJsonAsync<PagingResponse<CustomerDto>>(url, "Failed to query customers");
        }

        public Task<ResponseWrapper<CustomerDto>> GetCustomerByIdAsync(int id)
        {
            return _httpClient.GetWrapperFromJsonAsync<CustomerDto>($"{BaseUrl}/{id}", "Failed to retrieve customer");
        }

        public async Task<ResponseWrapper<CustomerDto>> CreateCustomerAsync(CustomerUpsertRequestDto customerDto)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, customerDto);
            return await response.EnsureSuccessAndReadWrapperAsync<CustomerDto>("Failed to create customer");
        }

        public async Task<ResponseWrapper<CustomerDto>> UpdateCustomerAsync(int id, CustomerUpsertRequestDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
            return await response.EnsureSuccessAndReadWrapperAsync<CustomerDto>("Failed to update customer");
        }

        public async Task<ResponseWrapper<bool>> DeleteCustomerAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return await response.EnsureSuccessAndReadWrapperAsync<bool>("Failed to delete customer");
        }
    }
}
