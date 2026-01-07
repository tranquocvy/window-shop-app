using FluentValidation.TestHelper; // Cần package FluentValidation.AspNetCore hoặc FluentValidation.DependencyInjectionExtensions
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Order.Commands.CreateOrder;
using TechHaven.Domain.Enums;
using TechHaven.Shared.DTOs.Orders;
using Xunit;

namespace TechHaven.UnitTests.Features.Orders;
[ExcludeFromCodeCoverage] //Đây chỉ là file test - không cần test file này

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator;

    public CreateOrderCommandValidatorTests()
    {
        _validator = new CreateOrderCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Discount_Is_Negative()
    {
        var command = new CreateOrderCommand { Discount = -10 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Discount);
    }

    [Fact] // Case Draft (Pending) -> Validation lỏng
    public void Should_Not_Have_Error_When_Status_Is_Pending_And_Customer_Is_Missing()
    {
        // Pending cho phép không có Customer (Walk-in guest hoặc draft)
        // Dựa theo code validator của bạn:
        // Rule: "When(x => x.Status != OrderStatus.Pending)" -> Nghĩa là Pending bỏ qua check Customer
        var command = new CreateOrderCommand
        {
            Status = Domain.Enums.OrderStatus.Pending,
            CustomerId = null
        };

        var result = _validator.TestValidate(command);

        // Assert không có lỗi ở CustomerId
        result.ShouldNotHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact] // Case Official (Completed/Processing) -> Validation chặt
    public void Should_Have_Error_When_Status_Is_Processing_And_Customer_Is_Missing()
    {
        var command = new CreateOrderCommand
        {
            Status = Domain.Enums.OrderStatus.Processing, // Không phải Pending
            CustomerId = null
        };

        var result = _validator.TestValidate(command);

        // Assert PHẢI có lỗi
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
              .WithErrorMessage("Customer is required for official orders.");
    }

    [Fact]
    public void Should_Have_Error_When_Details_Are_Empty_For_Official_Order()
    {
        var command = new CreateOrderCommand
        {
            Status = Domain.Enums.OrderStatus.Processing,
            Details = new List<OrderUpsertItemDto>() // Rỗng
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Details);
    }
}