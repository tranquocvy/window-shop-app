using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using TechHaven.Application.Features.Customer.Commands.CreateCustomer;
using TechHaven.Domain.Common;
using TechHaven.Domain.Enums; // Đảm bảo namespace chứa CustomerType
using TechHaven.Shared.DTOs.Customers;
using TechHaven.UnitTests.Common;
using Xunit;
using CustomerType = TechHaven.Shared.DTOs.Customers.CustomerType;

namespace TechHaven.UnitTests.Features.Customers;
[ExcludeFromCodeCoverage] //Đây chỉ là file test - không cần test file này

public class CreateCustomerCommandHandlerTests : UnitTestBase
{
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _handler = new CreateCustomerCommandHandler(MockUow.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ValidNewCustomer_ShouldCreateSuccessfully()
    {
        // --- ARRANGE ---
        var command = new CreateCustomerCommand
        {
            CustomerName = "Nguyen Van A",
            PhoneNumber = "0909123456",
            Email = "nguyenvana@example.com",
            Type = CustomerType.Regular, // Giả định enum Regular = 0 hoặc 1
            Address = "123 Street"
        };

        // 1. Mock Check trùng: KHÔNG tìm thấy khách hàng nào có sđt này (trả về null)
        MockCustomerRepo.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Domain.Entities.Customer, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Customer?)null);

        // 2. Mock AddAsync để trả về chính entity đó
        MockCustomerRepo.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Customer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Customer c, CancellationToken ct) => c);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.CustomerName.Should().Be("Nguyen Van A");
        result.Data.PhoneNumber.Should().Be("0909123456");

        // Verify: Add và SaveChanges được gọi 1 lần
        MockCustomerRepo.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Customer>(), It.IsAny<CancellationToken>()), Times.Once);
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicatePhoneNumber_ShouldReturnConflict()
    {
        // --- ARRANGE ---
        var phoneNumber = "0909123456";
        var command = new CreateCustomerCommand
        {
            CustomerName = "New Name",
            PhoneNumber = phoneNumber,
            Email = "new@example.com"
        };

        // 1. Mock Check trùng: TÌM THẤY khách hàng cũ
        var existingCustomer = new Domain.Entities.Customer
        {
            CustomerId = 1,
            CustomerName = "Old Name",
            PhoneNumber = phoneNumber
        };

        MockCustomerRepo.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Domain.Entities.Customer, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCustomer);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.Conflict); // Kiểm tra Conflict
        result.ErrorMessage.Should().Contain("already exists");

        // Verify: KHÔNG ĐƯỢC gọi Add hay SaveChanges
        MockCustomerRepo.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Customer>(), It.IsAny<CancellationToken>()), Times.Never);
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DatabaseException_ShouldReturnInternalError()
    {
        // --- ARRANGE ---
        var command = new CreateCustomerCommand { PhoneNumber = "0123456789" };

        // Giả lập lỗi khi truy vấn
        MockCustomerRepo.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Domain.Entities.Customer, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Connection Timeout"));

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.InternalError);
        result.ErrorMessage.Should().Contain("Failed to create customer");
    }
}