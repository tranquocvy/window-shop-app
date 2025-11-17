using System.Collections.Generic;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Orders;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ResponseWrapper<List<OrderDto>>> GetAllOrdersAsync();
        Task<ResponseWrapper<OrderDto>> GetOrderByIdAsync(int id);
        Task<ResponseWrapper<OrderDto>> CreateOrderAsync(OrderCreateDto dto);
        Task<ResponseWrapper<bool>> DeleteOrderAsync(int id);
        Task<ResponseWrapper<OrderDto>> UpdateOrderStatusAsync(OrderUpdateStatusDto dto);
        Task<ResponseWrapper<List<OrderDto>>> QueryOrdersAsync(OrderQueryDto query);
    }
}
