using FluentAssertions;
using Moq;
using TechHaven.Application.Features.Order.Commands.UpdateOrder;
using TechHaven.Domain.Common;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Enums;
using TechHaven.Shared.DTOs.Orders;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Orders;

public class UpdateOrderCommandHandlerTests : UnitTestBase
{
    private readonly UpdateOrderCommandHandler _handler;

    public UpdateOrderCommandHandlerTests()
    {
        _handler = new UpdateOrderCommandHandler(MockUow.Object, Mapper);
    }

    // --- TEST GROUP 1: BASIC VALIDATION & STATUS CHECKS ---

    [Fact]
    public async Task Handle_OrderNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var command = new UpdateOrderCommand { OrderId = 999 };
        MockOrderRepo.Setup(x => x.GetWithDetailsAsync(999, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Domain.Entities.Order?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_InvalidStatus_ShouldReturnValidationFailure()
    {
        // Arrange: Order đang ở trạng thái Cancelled -> Không được sửa
        var order = new Domain.Entities.Order { OrderId = 1, Status = Domain.Enums.OrderStatus.Cancelled };
        var command = new UpdateOrderCommand { OrderId = 1, Status = Domain.Enums.OrderStatus.Pending };

        MockOrderRepo.Setup(x => x.GetWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.Validation);
        result.ErrorMessage.Should().Contain("Only Pending or Processing");
    }

    // --- TEST GROUP 2: RETURN / REFUND LOGIC ---

    [Fact]
    public async Task Handle_StatusToReturned_ShouldRefundCustomer_And_Restock()
    {
        // Arrange
        var customerId = 10;
        var productId = 5;
        var order = new Domain.Entities.Order
        {
            OrderId = 1,
            Status = Domain.Enums.OrderStatus.Completed,
            CustomerId = customerId,
            TotalAmount = 500000,
            OrderDetails = new List<OrderDetail>
            {
                new() { ProductId = productId, Quantity = 2 }
            }
        };

        var customer = new Customer { CustomerId = customerId, TotalPurchased = 1000000 };
        var product = new Product { ProductId = productId, StockQuantity = 10 };

        var command = new UpdateOrderCommand { OrderId = 1, Status = Domain.Enums.OrderStatus.Returned };

        MockOrderRepo.Setup(x => x.GetWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        MockCustomerRepo.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        MockProductRepo.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(Domain.Enums.OrderStatus.Returned);

        // Check tiền hoàn: 1tr - 500k = 500k
        customer.TotalPurchased.Should().Be(500000);

        // Check hoàn kho: 10 + 2 = 12
        product.StockQuantity.Should().Be(12);

        // Đảm bảo SaveChanges được gọi 1 lần và return ngay
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- TEST GROUP 3: COMPLEX UPDATE LOGIC (Items, Stock, Customer Balance) ---

    [Fact]
    public async Task Handle_UpdateItems_ShouldAdjustStockAndRecalculateTotal()
    {
        // Arrange
        var productId = 1;
        // Đơn cũ: Mua 2 cái, giá 100k/cái = 200k total
        var order = new Domain.Entities.Order
        {
            OrderId = 1,
            Status = Domain.Enums.OrderStatus.Pending,
            CustomerId = 1,
            TotalAmount = 200000,
            OrderDetails = new List<OrderDetail>
            {
                new() { ProductId = productId, Quantity = 2, UnitPrice = 100000 }
            }
        };

        var product = new Product { ProductId = productId, StockQuantity = 20, SellPrice = 100000 };
        var customer = new Customer { CustomerId = 1, TotalPurchased = 200000 };

        // Request mới: Mua thành 5 cái (Tăng 3 cái)
        var command = new UpdateOrderCommand
        {
            OrderId = 1,
            CustomerId = 1,
            Status = Domain.Enums.OrderStatus.Pending,
            Details = new List<OrderUpsertItemDto>
            {
                new() { ProductId = productId, Quantity = 5 }
            }
        };

        MockOrderRepo.Setup(x => x.GetWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        MockProductRepo.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        MockCustomerRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // 1. Check Kho: Tăng 3 cái -> Kho giảm 3 (20 - 3 = 17)
        product.StockQuantity.Should().Be(17);

        // 2. Check Order Total: 5 cái * 100k = 500k
        order.TotalAmount.Should().Be(500000);

        // 3. Check Customer Total: Cũ 200k - Đơn cũ 200k + Đơn mới 500k = 500k
        customer.TotalPurchased.Should().Be(500000);
    }

    [Fact]
    public async Task Handle_RemoveItem_ShouldRestockProduct()
    {
        // Arrange
        var productId = 1;
        var order = new Domain.Entities.Order
        {
            OrderId = 1,
            Status = Domain.Enums.OrderStatus.Pending,
            OrderDetails = new List<OrderDetail> { new() { ProductId = productId, Quantity = 5 } }
        };
        var product = new Product { ProductId = productId, StockQuantity = 10 };

        // Request rỗng -> Xóa hết item
        var command = new UpdateOrderCommand { OrderId = 1, Details = new List<OrderUpsertItemDto>() };

        MockOrderRepo.Setup(x => x.GetWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        MockProductRepo.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        // Item bị xóa khỏi list
        order.OrderDetails.Should().BeEmpty();
        // Kho được hoàn: 10 + 5 = 15
        product.StockQuantity.Should().Be(15);
    }

    [Fact]
    public async Task Handle_DecreaseQuantity_ShouldRestockPartial()
    {
        // Arrange: Đang mua 5, giảm xuống còn 2
        var productId = 1;
        var order = new Domain.Entities.Order
        {
            OrderId = 1,
            Status = Domain.Enums.OrderStatus.Pending,
            OrderDetails = new List<OrderDetail> { new() { ProductId = productId, Quantity = 5 } }
        };
        var product = new Product { ProductId = productId, StockQuantity = 10 };

        var command = new UpdateOrderCommand
        {
            OrderId = 1,
            Details = new List<OrderUpsertItemDto> { new() { ProductId = productId, Quantity = 2 } }
        };

        MockOrderRepo.Setup(x => x.GetWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        MockProductRepo.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        // Giảm 3 cái -> Kho tăng 3 (10 + 3 = 13)
        product.StockQuantity.Should().Be(13);
    }

    // --- TEST GROUP 4: DISCOUNT LOGIC (If/Else Coverage) ---

    [Fact]
    public async Task Handle_DiscountPercentage_ShouldCalculateCorrectly()
    {
        // Arrange: Discount = 0.1 (10%)
        var productId = 1;
        var order = new Domain.Entities.Order { OrderId = 1, Status = Domain.Enums.OrderStatus.Pending };
        var product = new Product { ProductId = productId, SellPrice = 100000, StockQuantity = 100 };

        var command = new UpdateOrderCommand
        {
            OrderId = 1,
            Discount = 0.1m, // 10%
            Details = new List<OrderUpsertItemDto> { new() { ProductId = productId, Quantity = 1 } }
        };

        MockOrderRepo.Setup(x => x.GetWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        MockProductRepo.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        // Subtotal = 100k. Discount 10% = 10k. Total = 90k.
        order.TotalAmount.Should().Be(90000);
    }

    [Fact]
    public async Task Handle_DiscountFixedAmount_ShouldCalculateCorrectly()
    {
        // Arrange: Discount = 20000 (Tiền mặt)
        var productId = 1;
        var order = new Domain.Entities.Order { OrderId = 1, Status = Domain.Enums.OrderStatus.Pending };
        var product = new Product { ProductId = productId, SellPrice = 100000, StockQuantity = 100 };

        var command = new UpdateOrderCommand
        {
            OrderId = 1,
            Discount = 20000, // Trừ thẳng 20k
            Details = new List<OrderUpsertItemDto> { new() { ProductId = productId, Quantity = 1 } }
        };

        MockOrderRepo.Setup(x => x.GetWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        MockProductRepo.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        // Subtotal = 100k. Discount = 20k. Total = 80k.
        order.TotalAmount.Should().Be(80000);
    }

    // --- TEST GROUP 5: CHANGE CUSTOMER LOGIC ---

    [Fact]
    public async Task Handle_ChangeCustomer_ShouldUpdateBothBalances()
    {
        // Arrange
        var oldCustId = 1;
        var newCustId = 2;
        var productId = 1;

        var order = new Domain.Entities.Order
        {
            OrderId = 1,
            Status = Domain.Enums.OrderStatus.Pending,
            CustomerId = oldCustId,
            TotalAmount = 50000,
            OrderDetails = new List<OrderDetail> { new() { ProductId = productId, Quantity = 1 } }
        };
        var oldCust = new Customer { CustomerId = oldCustId, TotalPurchased = 50000 };
        var newCust = new Customer { CustomerId = newCustId, TotalPurchased = 0 };
        var product = new Product { ProductId = productId, SellPrice = 50000, StockQuantity = 10 };

        var command = new UpdateOrderCommand
        {
            OrderId = 1,
            CustomerId = newCustId,
            Details = new List<OrderUpsertItemDto> { new() { ProductId = productId, Quantity = 1 } }
        };

        MockOrderRepo.Setup(x => x.GetWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        MockCustomerRepo.Setup(x => x.GetByIdAsync(oldCustId, It.IsAny<CancellationToken>())).ReturnsAsync(oldCust);
        MockCustomerRepo.Setup(x => x.GetByIdAsync(newCustId, It.IsAny<CancellationToken>())).ReturnsAsync(newCust);
        MockProductRepo.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        oldCust.TotalPurchased.Should().Be(0);      // Trừ đi 50k
        newCust.TotalPurchased.Should().Be(50000);  // Cộng thêm 50k
    }
}