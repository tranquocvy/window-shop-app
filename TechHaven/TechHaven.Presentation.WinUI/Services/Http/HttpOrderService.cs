using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Orders;

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

        public Task<ResponseWrapper<PagingResponse<OrderDto>>> GetOrdersAsync(OrderListQueryDto query)
        {
            var queryString = BuildQueryString(query);
            var url = string.IsNullOrEmpty(queryString) ? BaseUrl : $"{BaseUrl}?{queryString}";
            return _httpClient.GetWrapperFromJsonAsync<PagingResponse<OrderDto>>(url, "Failed to retrieve orders");
        }

        public Task<ResponseWrapper<OrderDto>> GetOrderByIdAsync(int id)
        {
            return _httpClient.GetWrapperFromJsonAsync<OrderDto>($"{BaseUrl}/{id}", "Failed to retrieve order");
        }

        public async Task<ResponseWrapper<OrderDto>> CreateOrderAsync(OrderUpsertRequestDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);
            return await response.EnsureSuccessAndReadWrapperAsync<OrderDto>("Failed to create order");
        }

        public async Task<ResponseWrapper<bool>> DeleteOrderAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return await response.EnsureSuccessAndReadWrapperAsync<bool>("Failed to delete order");
        }

        public async Task<ResponseWrapper<OrderDto>> UpdateOrderAsync(int orderId, OrderUpsertRequestDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{orderId}", dto);
            return await response.EnsureSuccessAndReadWrapperAsync<OrderDto>("Failed to update order");
        }

        private static string BuildQueryString(OrderListQueryDto query)
        {
            var sb = new StringBuilder();

            if (query.PageNumber > 0)
                sb.Append($"PageNumber={query.PageNumber}&");
            
            if (query.PageSize > 0)
                sb.Append($"PageSize={query.PageSize}&");

            if (query.Status.HasValue)
                sb.Append($"Status={query.Status.Value}&");

            if (query.OrderDate?.StartDate.HasValue == true)
                sb.Append($"OrderDate.StartDate={query.OrderDate.StartDate.Value:yyyy-MM-ddTHH:mm:ss}&");

            if (query.OrderDate?.EndDate.HasValue == true)
                sb.Append($"OrderDate.EndDate={query.OrderDate.EndDate.Value:yyyy-MM-ddTHH:mm:ss}&");

            if (!string.IsNullOrWhiteSpace(query.CustomerKeyword))
                sb.Append($"CustomerKeyword={HttpUtility.UrlEncode(query.CustomerKeyword)}&");

            if (query.Sorting != null)
            {
                if (!string.IsNullOrWhiteSpace(query.Sorting.SortBy))
                    sb.Append($"Sorting.SortBy={query.Sorting.SortBy}&");
                
                if (query.Sorting.Desc)
                    sb.Append($"Sorting.Desc={query.Sorting.Desc}&");
            }

            // Remove trailing &
            if (sb.Length > 0 && sb[sb.Length - 1] == '&')
                sb.Length--;

            return sb.ToString();
        }
    }
}
