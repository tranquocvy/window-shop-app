using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Orders;
using TechHaven.Domain.Common;
using TechHaven.Domain.Enums;
using OrderStatus = TechHaven.Domain.Enums.OrderStatus;

namespace TechHaven.Application.Features.Order.Commands.UpdateOrder;

public record UpdateOrderCommand : ICommand<Result<OrderDto>>
{
    // Id lấy từ URL (route param), controller sẽ gán vào đây
    public int OrderId { get; set; }

    // Các trường update (map từ OrderUpsertRequestDto)
    public int? CustomerId { get; init; }
    public OrderStatus Status { get; init; }
    public decimal Discount { get; init; } // 0.0 đến 1.0
    public string? Notes { get; init; }

    // Danh sách sản phẩm mới user muốn cập nhật
    public IReadOnlyList<OrderUpsertItemDto> Details { get; init; } = Array.Empty<OrderUpsertItemDto>();
}