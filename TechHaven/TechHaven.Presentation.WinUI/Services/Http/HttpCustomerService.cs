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
using System.Text.Json;

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
            if (customerDto == null) throw new ArgumentNullException(nameof(customerDto));

            var url = BaseUrl;

            try
            {
                Debug.WriteLine($"HttpCustomerService.CreateCustomerAsync -> POST URL: {_httpClient.BaseAddress?.ToString().TrimEnd('/')}/{url}");
                Debug.WriteLine($"HttpCustomerService.CreateCustomerAsync -> Payload: {JsonSerializer.Serialize(customerDto)}");

                var response = await _httpClient.PostAsJsonAsync(url, customerDto);

                // Read wrapper via extension (this safely reads content) and log details for debugging
                var wrapper = await response.EnsureSuccessAndReadWrapperAsync<CustomerDto>("Failed to create customer");

                Debug.WriteLine($"CreateCustomerAsync -> HTTP {(int)response.StatusCode} {response.ReasonPhrase}, Success={wrapper?.Success}");
                if (wrapper != null)
                {
                    Debug.WriteLine($"CreateCustomerAsync -> Message: {wrapper.Message}");
                    if (wrapper.Errors != null && wrapper.Errors.Count > 0)
                    {
                        foreach (var err in wrapper.Errors)
                            Debug.WriteLine($"CreateCustomerAsync -> Error: {err}");
                    }
                }

                return wrapper;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in CreateCustomerAsync: {ex}");
                throw;
            }
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
