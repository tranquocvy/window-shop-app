using TechHaven.Shared.DTOs.Orders;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Enums;
using OrderStatus = TechHaven.Domain.Enums.OrderStatus;

namespace TechHaven.Application.Features.Order.Commands.CreateOrder
{
    public record class CreateOrderCommand : ICommand<Result<OrderDto>> //vì response là OrderDto nên sẽ lấy kiểu T làm chuẩn
    {
        public int OrderId { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int UserId { get; set; }
        public string? UserFullName { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public IReadOnlyList<OrderUpsertItemDto> Details { get; set; } = Array.Empty<OrderUpsertItemDto>();

    }
}
