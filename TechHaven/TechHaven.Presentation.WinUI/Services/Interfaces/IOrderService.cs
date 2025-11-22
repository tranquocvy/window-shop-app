using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Orders;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ResponseWrapper<PagingResponse<OrderDto>>> GetOrdersAsync(OrderListQueryDto query);
        
        Task<ResponseWrapper<OrderDto>> GetOrderByIdAsync(int id);
        
        Task<ResponseWrapper<OrderDto>> CreateOrderAsync(OrderUpsertRequestDto dto);
        
        Task<ResponseWrapper<bool>> DeleteOrderAsync(int id);
        
        Task<ResponseWrapper<OrderDto>> UpdateOrderAsync(int orderId, OrderUpsertRequestDto dto);
    }
}
