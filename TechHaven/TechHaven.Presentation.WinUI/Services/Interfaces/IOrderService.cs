using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Orders;
namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<OrderDto> CreateOrderAsync(OrderCreateDto dto);
        Task<bool> DeleteOrderAsync(int id);
        Task<OrderDto?> UpdateOrderStatusAsync(OrderUpdateStatusDto dto);
        Task<List<OrderDto>> QueryOrdersAsync(OrderQueryDto query);
    }
}
