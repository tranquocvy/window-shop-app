using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Customer.Commands.UpdateCustomer;
using TechHaven.Domain.Common;
using TechHaven.Domain.Enums; // Namespace chứa CustomerType
using TechHaven.UnitTests.Common;
using Xunit;
using TechHaven.Shared.DTOs.Customers;

namespace TechHaven.UnitTests.Features.Customers;

[ExcludeFromCodeCoverage] // Đây là file test logic Update Customer, không tính coverage
public class UpdateCustomerCommandHandlerTests : UnitTestBase
{
    private readonly UpdateCustomerCommandHandler _handler;

    public UpdateCustomerCommandHandlerTests()
    {
        _handler = new UpdateCustomerCommandHandler(MockUow.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ExistingCustomer_ShouldUpdateFields_And_SetUpdatedAt()
    {
        // --- ARRANGE ---
        var customerId = 1;

        // Data cũ trong DB
        var existingCustomer = new Domain.Entities.Customer
        {
            CustomerId = customerId,
            CustomerName = "Old Name",
            PhoneNumber = "0900000000",
            UpdatedAt = null
        };

        // Data mới từ Request
        var command = new UpdateCustomerCommand
        {
            CustomerId = customerId,
            CustomerName = "New Name",
            PhoneNumber = "0999999999",
            Email = "new@example.com",
            Address = "New Address",
            Type = Shared.DTOs.Customers.CustomerType.VIP,
            Note = "Updated Note"
        };

        // Mock tìm thấy
        MockCustomerRepo.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeTrue();

        // 1. Kiểm tra Entity trong Memory đã đổi chưa
        existingCustomer.CustomerName.Should().Be("New Name");
        existingCustomer.PhoneNumber.Should().Be("0999999999");
        existingCustomer.Type.Should().Be(Domain.Enums.CustomerType.VIP);

        // 2. Kiểm tra UpdatedAt đã được set chưa
        existingCustomer.UpdatedAt.Should().NotBeNull();
        existingCustomer.UpdatedAt.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // 3. Kiểm tra DTO trả về
        result.Data.Should().NotBeNull();
        result.Data.CustomerName.Should().Be("New Name");

        // Verify gọi Update và SaveChanges
        MockCustomerRepo.Verify(x => x.UpdateAsync(existingCustomer, It.IsAny<CancellationToken>()), Times.Once); // Thêm CancellationToken vào Verify
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ShouldReturnNotFound()
    {
        // --- ARRANGE ---
        var command = new UpdateCustomerCommand { CustomerId = 999, CustomerName = "Ghost" };

        MockCustomerRepo.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Customer?)null);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.NotFound);
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_Exception_ShouldReturnInternalError()
    {
        // --- ARRANGE ---
        var customerId = 1;
        var existingCustomer = new Domain.Entities.Customer { CustomerId = customerId };
        var command = new UpdateCustomerCommand { CustomerId = customerId };

        MockCustomerRepo.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        MockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection lost"));

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.InternalError);
    }
}