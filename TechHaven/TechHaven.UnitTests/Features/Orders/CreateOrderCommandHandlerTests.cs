using Bogus;
using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Order.Commands.CreateOrder;
using TechHaven.Domain.Common;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Enums;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Orders;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Orders;
[ExcludeFromCodeCoverage] //Đây chỉ là file test - không cần test file này

public class CreateOrderCommandHandlerTests : UnitTestBase
{
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        // Khởi tạo Handler với Mock dependencies từ UnitTestBase
        _handler = new CreateOrderCommandHandler(MockUow.Object, Mapper);
    }

    [Fact] // Happy Path
    public async Task Handle_ValidOrder_ShouldSucceed_And_ReduceStock_And_IncreaseCustomerTotal()
    {
        // --- ARRANGE ---
        // 1. Tạo Data giả lập
        var customer = DataGenerator.CustomerFaker.Generate();
        customer.TotalPurchased = 0; // Reset về 0 để dễ tính toán

        var product = DataGenerator.ProductFaker.Generate();
        product.StockQuantity = 100; // Kho đang có 100
        product.SellPrice = 200*1000;     // Giá bán 200k

        // 2. Tạo Command (Request từ Client)
        var command = new CreateOrderCommand
        {
            CustomerId = customer.CustomerId,
            UserId = 1,
            Status = Domain.Enums.OrderStatus.Pending,
            Discount = 0, // Không giảm giá
            Details = new List<OrderUpsertItemDto>
            {
                new() { ProductId = product.ProductId, Quantity = 5 } // Mua 5 cái
            }
        };

        // 3. Setup Mock Behavior
        // Khi Handler gọi GetByIdAsync, trả về object giả lập
        MockCustomerRepo.Setup(x => x.GetByIdAsync(customer.CustomerId, It.IsAny<CancellationToken>())) //Expression tree không hỗ trợ overload có default param -> Buộc phải khai báo tường minh cho CancellationToken
                        .ReturnsAsync(customer);

        MockProductRepo.Setup(x => x.GetByIdAsync(product.ProductId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(product);

        // Setup AddAsync ( có thể không cần return gì đặc biệt nhưng hàm AddAsync bắt buộc phải trả về một object dữ liệu Task<Order> thay vì Task.Completed - dữ liệu void)
        MockOrderRepo.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Order>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Domain.Entities.Order order, CancellationToken ct) => order);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        // 1. Kiểm tra kết quả trả về
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.TotalAmount.Should().Be(200*5*1000); // VND

        // 2. Kiểm tra Logic Trừ Kho (Stock Error Check)
        product.StockQuantity.Should().Be(95); // 100 - 5 = 95

        // 3. Kiểm tra Logic Cộng Tiền Khách (Customer Integrity)
        customer.TotalPurchased.Should().Be(1000000); // 0 + 200*5*1000 = 1000000 VND

        // 4. Kiểm tra xem đã gọi SaveChangesAsync chưa
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact] // Stock Error Scenario
    public async Task Handle_InsufficientStock_ShouldReturnFailure_And_NotSaveToDB()
    {
        // --- ARRANGE ---
        var product = DataGenerator.ProductFaker.Generate();
        product.StockQuantity = 2; // Kho chỉ còn 2

        var command = new CreateOrderCommand
        {
            UserId = 1,
            Details = new List<OrderUpsertItemDto>
            {
                new() { ProductId = product.ProductId, Quantity = 10 } // Đòi mua 10
            }
        };

        MockProductRepo.Setup(x => x.GetByIdAsync(product.ProductId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(product);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.Validation); //Sửa result.ErrorType -> result.ErrorCode
        result.ErrorMessage.Should().Contain("insufficient stock");

        // QUAN TRỌNG: Đảm bảo không gọi SaveChanges (Không lưu rác vào DB)
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact] // Price Integrity Scenario
    public async Task Handle_ClientSendsWrongPrice_ShouldUseDatabasePrice()
    {
        // --- ARRANGE ---
        var product = DataGenerator.ProductFaker.Generate();
        product.SellPrice = 500*1000; // Giá thật trong DB

        var command = new CreateOrderCommand
        {
            UserId = 1,
            Details = new List<OrderUpsertItemDto>
            {
                // Client gian lận, cố tình gửi giá 1 đồng (Lưu ý: DTO ko có field UnitPrice, nhưng giả sử có logic ngầm)
                // Logic Handler của bạn không đọc giá từ DTO, điều này tốt. 
                // Test này verify logic đó vẫn hoạt động đúng.
                new() { ProductId = product.ProductId, Quantity = 1 }
            }
        };

        MockProductRepo.Setup(x => x.GetByIdAsync(product.ProductId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeTrue();
        // Kiểm tra Subtotal phải tính theo giá 500 (DB) chứ không phải giá nào khác
        result.Data!.SubtotalAmount.Should().Be(500*1000);
    }

    [Fact] // Customer Integrity Scenario
    public async Task Handle_CustomerNotFound_ShouldReturnNotFound()
    {
        // --- ARRANGE ---
        var command = new CreateOrderCommand { CustomerId = 999 }; // ID không tồn tại

        MockCustomerRepo.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Customer?)null); // Trả về null

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.NotFound);
    }

    [Theory]
    [InlineData(0.1, 1000000, 900000)] // Giảm 10% của 1tr -> Còn 900k
    [InlineData(50000, 1000000, 950000)] // Giảm 50k của 1tr -> Còn 950k
    [InlineData(2000000, 1000000, 0)]  // Giảm 2tr (quá tiền hàng) -> Còn 0
    public async Task Handle_DiscountLogic_ShouldCalculateTotalCorrectly(decimal discountInput, decimal subTotalInput, decimal expectedTotal)
    {
        // --- ARRANGE ---
        var product = DataGenerator.ProductFaker.Generate();
        product.SellPrice = subTotalInput; // Giá SP bằng đúng subtotal để test cho dễ
        product.StockQuantity = 100;

        var command = new CreateOrderCommand
        {
            UserId = 1,
            Discount = discountInput,
            Details = new List<OrderUpsertItemDto>
            {
                new() { ProductId = product.ProductId, Quantity = 1 }
            }
        };

        MockProductRepo.Setup(x => x.GetByIdAsync(product.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeTrue();
        result.Data!.TotalAmount.Should().Be(expectedTotal);
    }
}