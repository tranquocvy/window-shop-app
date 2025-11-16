using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Orders;
using Shared.DTOs.Common;

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
