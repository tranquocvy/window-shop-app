using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Customer.Commands.DeleteCustomer;
using TechHaven.Domain.Common;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Customers;

[ExcludeFromCodeCoverage] // Đây là file test logic Delete Customer, không tính coverage
public class DeleteCustomerCommandHandlerTests : UnitTestBase
{
    private readonly DeleteCustomerCommandHandler _handler;

    public DeleteCustomerCommandHandlerTests()
    {
        _handler = new DeleteCustomerCommandHandler(MockUow.Object);
    }

    [Fact]
    public async Task Handle_ExistingCustomer_ShouldDeleteSuccessfully()
    {
        // --- ARRANGE ---
        var customerId = 1;
        var customer = new Domain.Entities.Customer { CustomerId = customerId, CustomerName = "Test Customer" };

        // Giả sử Command dùng Object Initializer
        var command = new DeleteCustomerCommand(customerId);

        // 1. Mock tìm thấy khách hàng
        MockCustomerRepo.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeTrue();

        // Verify gọi Delete và SaveChanges
        MockCustomerRepo.Verify(x => x.DeleteAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ShouldReturnNotFound()
    {
        // --- ARRANGE ---
        var command = new DeleteCustomerCommand(999);

        // Mock không tìm thấy (return null)
        MockCustomerRepo.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Customer?)null);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.NotFound);
        result.ErrorMessage.Should().Contain("not found");

        // Verify KHÔNG gọi Delete
        MockCustomerRepo.Verify(x => x.DeleteAsync(It.IsAny<Domain.Entities.Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Exception_ShouldReturnInternalError()
    {
        // --- ARRANGE ---
        var customerId = 1;
        var customer = new Domain.Entities.Customer { CustomerId = customerId };
        var command = new DeleteCustomerCommand(customerId);

        MockCustomerRepo.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Giả lập lỗi DB
        MockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Error"));

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.InternalError);
    }
}