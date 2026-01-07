using FluentValidation.TestHelper;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Customer.Commands.CreateCustomer;
using TechHaven.Domain.Enums; // Namespace chứa Enum CustomerType
using TechHaven.Shared.DTOs.Customers;
using Xunit;

namespace TechHaven.UnitTests.Features.Customers;
[ExcludeFromCodeCoverage] //Đây chỉ là file test - không cần test file này

public class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _validator;

    public CreateCustomerCommandValidatorTests()
    {
        _validator = new CreateCustomerCommandValidator();
    }

    // --- Happy Path ---
    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new CreateCustomerCommand
        {
            CustomerName = "Valid Name",
            PhoneNumber = "0909-123-456", // Format cho phép dấu gạch ngang
            Email = "valid@test.com",
            Type = Shared.DTOs.Customers.CustomerType.VIP, // Giả sử có enum VIP
            Address = "Valid Address",
            Note = "Valid Note"
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- Required Fields ---
    [Fact]
    public void Validate_MissingRequiredFields_ShouldFail()
    {
        var command = new CreateCustomerCommand
        {
            CustomerName = "", // Empty
            PhoneNumber = ""   // Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    // --- Regex Validation (Phone) ---
    [Theory]
    [InlineData("0909abc")]     // Chứa chữ cái
    [InlineData("0909@123")]    // Chứa ký tự đặc biệt không cho phép
    public void Validate_InvalidPhoneFormat_ShouldFail(string invalidPhone)
    {
        var command = new CreateCustomerCommand { PhoneNumber = invalidPhone };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
              .WithErrorMessage("Phone number contains invalid characters.");
    }

    [Theory]
    [InlineData("0909123456")]      // Số thuần
    [InlineData("+84909123456")]    // Có dấu cộng
    [InlineData("(028) 3939")]      // Có ngoặc và khoảng trắng
    [InlineData("090-123-456")]     // Có gạch ngang
    public void Validate_ValidPhoneFormat_ShouldPass(string validPhone)
    {
        var command = new CreateCustomerCommand
        {
            CustomerName = "Test",
            PhoneNumber = validPhone,
            Type = Shared.DTOs.Customers.CustomerType.Regular
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    // --- Email Validation ---
    [Fact]
    public void Validate_InvalidEmail_ShouldFail()
    {
        var command = new CreateCustomerCommand { Email = "invalid-email" }; // Thiếu @ và domain
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_EmptyEmail_ShouldPass()
    {
        // Email là optional (khi dùng When trong Validator), nên rỗng phải pass
        var command = new CreateCustomerCommand
        {
            CustomerName = "Test",
            PhoneNumber = "0909",
            Type = Shared.DTOs.Customers.CustomerType.Regular,
            Email = "" // Empty
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    // --- Length Checks ---
    [Fact]
    public void Validate_StringLengthExceeded_ShouldFail()
    {
        var longName = new string('a', 151); // Max 150
        var longPhone = new string('1', 16); // Max 15

        var command = new CreateCustomerCommand
        {
            CustomerName = longName,
            PhoneNumber = longPhone
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }
}