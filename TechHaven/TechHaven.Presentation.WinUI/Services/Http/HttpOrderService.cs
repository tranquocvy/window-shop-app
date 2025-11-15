using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Orders;
using TechHaven.Presentation.WinUI.Services.Interfaces;

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

        public async Task<ResponseWrapper<List<OrderDto>>> GetAllOrdersAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ResponseWrapper<List<OrderDto>>>(BaseUrl);
                return response ?? new ResponseWrapper<List<OrderDto>> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<List<OrderDto>> { Success = false, Message = "Failed to retrieve orders", Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResponseWrapper<OrderDto>> GetOrderByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ResponseWrapper<OrderDto>>($"{BaseUrl}/{id}");
                return response ?? new ResponseWrapper<OrderDto> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<OrderDto> { Success = false, Message = "Failed to retrieve order", Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResponseWrapper<OrderDto>> CreateOrderAsync(OrderCreateDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);
                if (!response.IsSuccessStatusCode)
                    return new ResponseWrapper<OrderDto> { Success = false, Message = $"API returned {(int)response.StatusCode}" };

                var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<OrderDto>>();
                return wrapper ?? new ResponseWrapper<OrderDto> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<OrderDto> { Success = false, Message = "Failed to create order", Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResponseWrapper<bool>> DeleteOrderAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                if (!response.IsSuccessStatusCode)
                    return new ResponseWrapper<bool> { Success = false, Message = $"API returned {(int)response.StatusCode}" };

                var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<bool>>();
                return wrapper ?? new ResponseWrapper<bool> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<bool> { Success = false, Message = "Failed to delete order", Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResponseWrapper<OrderDto>> UpdateOrderStatusAsync(OrderUpdateStatusDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/status", dto);
                if (!response.IsSuccessStatusCode)
                    return new ResponseWrapper<OrderDto> { Success = false, Message = $"API returned {(int)response.StatusCode}" };

                var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<OrderDto>>();
                return wrapper ?? new ResponseWrapper<OrderDto> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<OrderDto> { Success = false, Message = "Failed to update order status", Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResponseWrapper<List<OrderDto>>> QueryOrdersAsync(OrderQueryDto query)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/query", query);
                if (!response.IsSuccessStatusCode)
                    return new ResponseWrapper<List<OrderDto>> { Success = false, Message = $"API returned {(int)response.StatusCode}" };

                var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<List<OrderDto>>>();
                return wrapper ?? new ResponseWrapper<List<OrderDto>> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<List<OrderDto>> { Success = false, Message = "Failed to query orders", Errors = new List<string> { ex.Message } };
            }
        }
    }
}
