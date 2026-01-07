using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Order.Commands.DeleteOrder;
using TechHaven.Domain.Common;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Enums;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Orders;
[ExcludeFromCodeCoverage] //Đây chỉ là file test - không cần test file này

public class DeleteOrderCommandHandlerTests : UnitTestBase
{
    private readonly DeleteOrderCommandHandler _handler;

    public DeleteOrderCommandHandlerTests()
    {
        _handler = new DeleteOrderCommandHandler(MockUow.Object);
    }

    [Fact] // Happy Path
    public async Task Handle_PendingOrder_ShouldRestock_RefundCustomer_AndDelete()
    {
        // --- ARRANGE ---
        var orderId = 1;
        var customerId = 10;
        var productId = 99;

        // 1. Chuẩn bị dữ liệu Product và Customer
        var product = new Product
        {
            ProductId = productId,
            StockQuantity = 10, // Kho đang có 10
            ProductName = "Test Phone"
        };

        var customer = new Customer
        {
            CustomerId = customerId,
            TotalPurchased = 500000 // Khách đã mua 500k
        };

        // 2. Chuẩn bị Order cần xóa
        var order = new Domain.Entities.Order
        {
            OrderId = orderId,
            CustomerId = customerId,
            Status = OrderStatus.Pending, // Trạng thái được phép xóa
            TotalAmount = 200000, // Giá trị đơn hàng 200k
            OrderDetails = new List<OrderDetail>
            {
                new()
                {
                    ProductId = productId,
                    Quantity = 2,
                    Product = product // [QUAN TRỌNG] Phải link tay object Product vào đây
                }
            }
        };

        // 3. Setup Mock
        MockOrderRepo.Setup(x => x.GetWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(order); // Trả về Order đã kèm Product

        MockCustomerRepo.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(customer);

        // --- ACT ---
        var result = await _handler.Handle(new DeleteOrderCommand(orderId), CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeTrue();

        // 1. Kiểm tra Restock (Hoàn kho)
        // Ban đầu 10, đơn hàng có 2 -> Xóa đơn thì kho phải về 12
        product.StockQuantity.Should().Be(12);

        // 2. Kiểm tra Refund (Hoàn tiền tích lũy)
        // Ban đầu 500k, đơn hàng 200k -> Xóa đơn thì khách còn 300k
        customer.TotalPurchased.Should().Be(300000);

        // 3. Kiểm tra lệnh Delete đã được gọi xuống Repo
        MockOrderRepo.Verify(x => x.DeleteAsync(order, It.IsAny<CancellationToken>()), Times.Once);

        // 4. Kiểm tra SaveChanges
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact] // Validation Error Scenario
    public async Task Handle_CompletedOrder_ShouldReturnFailure_And_NotDelete()
    {
        // --- ARRANGE ---
        var orderId = 1;
        var order = new Domain.Entities.Order
        {
            OrderId = orderId,
            Status = OrderStatus.Completed // Trạng thái KHÔNG được phép xóa
        };

        MockOrderRepo.Setup(x => x.GetWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(order);

        // --- ACT ---
        var result = await _handler.Handle(new DeleteOrderCommand(orderId), CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.Validation);
        result.ErrorMessage.Should().Contain("Cannot delete order");

        // Đảm bảo KHÔNG gọi lệnh xóa
        MockOrderRepo.Verify(x => x.DeleteAsync(It.IsAny<Domain.Entities.Order>(), It.IsAny<CancellationToken>()), Times.Never);
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact] // Not Found Scenario
    public async Task Handle_OrderNotFound_ShouldReturnNotFound()
    {
        // --- ARRANGE ---
        var orderId = 999;
        MockOrderRepo.Setup(x => x.GetWithDetailsAsync(orderId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Domain.Entities.Order?)null); // Trả về null

        // --- ACT ---
        var result = await _handler.Handle(new DeleteOrderCommand(orderId), CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.NotFound);
    }
}