using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Orders;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Presentation.WinUI.Helpers;

namespace TechHaven.Presentation.WinUI.Services.Http
{
    public class HttpOrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/orders";

        public HttpOrderService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public Task<ResponseWrapper<List<OrderDto>>> GetAllOrdersAsync()
        {
            return _httpClient.GetWrapperFromJsonAsync<List<OrderDto>>(BaseUrl, "Failed to retrieve orders");
        }

        public Task<ResponseWrapper<OrderDto>> GetOrderByIdAsync(int id)
        {
            return _httpClient.GetWrapperFromJsonAsync<OrderDto>($"{BaseUrl}/{id}", "Failed to retrieve order");
        }

        public async Task<ResponseWrapper<OrderDto>> CreateOrderAsync(OrderCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);
            return await response.EnsureSuccessAndReadWrapperAsync<OrderDto>("Failed to create order");
        }

        public async Task<ResponseWrapper<bool>> DeleteOrderAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return await response.EnsureSuccessAndReadWrapperAsync<bool>("Failed to delete order");
        }

        public async Task<ResponseWrapper<OrderDto>> UpdateOrderStatusAsync(OrderUpdateStatusDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/status", dto);
            return await response.EnsureSuccessAndReadWrapperAsync<OrderDto>("Failed to update order status");
        }

        public async Task<ResponseWrapper<List<OrderDto>>> QueryOrdersAsync(OrderQueryDto query)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/query", query);
            return await response.EnsureSuccessAndReadWrapperAsync<List<OrderDto>>("Failed to query orders");
        }
    }
}
