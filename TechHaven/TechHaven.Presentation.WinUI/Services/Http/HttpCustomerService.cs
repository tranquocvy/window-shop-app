using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Customers;

namespace TechHaven.Presentation.WinUI.Services.Http
{
    public class HttpCustomerService : ICustomerService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/customers";

        public HttpCustomerService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public Task<ResponseWrapper<List<CustomerDto>>> GetAllCustomersAsync()
        {
            return _httpClient.GetWrapperFromJsonAsync<List<CustomerDto>>(BaseUrl, "Failed to retrieve customers");
        }

        public Task<ResponseWrapper<CustomerDto>> GetCustomerByIdAsync(int id)
        {
            return _httpClient.GetWrapperFromJsonAsync<CustomerDto>($"{BaseUrl}/{id}", "Failed to retrieve customer");
        }

        public async Task<ResponseWrapper<CustomerDto>> CreateCustomerAsync(CustomerCreateUpdateDto customerDto)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, customerDto);
            return await response.EnsureSuccessAndReadWrapperAsync<CustomerDto>("Failed to create customer");
        }

        public async Task<ResponseWrapper<CustomerDto>> UpdateCustomerAsync(int id, CustomerCreateUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
            return await response.EnsureSuccessAndReadWrapperAsync<CustomerDto>("Failed to update customer");
        }

        public async Task<ResponseWrapper<bool>> DeleteCustomerAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return await response.EnsureSuccessAndReadWrapperAsync<bool>("Failed to delete customer");
        }

        public async Task<ResponseWrapper<List<CustomerDto>>> QueryCustomersAsync(CustomerQueryDto query)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/query", query);
            return await response.EnsureSuccessAndReadWrapperAsync<List<CustomerDto>>("Failed to query customers");
        }
    }
}
