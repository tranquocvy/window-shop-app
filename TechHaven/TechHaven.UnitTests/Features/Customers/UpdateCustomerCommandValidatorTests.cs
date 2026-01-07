using FluentValidation.TestHelper;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Customer.Commands.UpdateCustomer;
using TechHaven.Domain.Enums;
using Xunit;
using TechHaven.Shared.DTOs.Customers;

namespace TechHaven.UnitTests.Features.Customers;

[ExcludeFromCodeCoverage] // Đây là file test Validator Update Customer, không tính coverage
public class UpdateCustomerCommandValidatorTests
{
    private readonly UpdateCustomerCommandValidator _validator;

    public UpdateCustomerCommandValidatorTests()
    {
        _validator = new UpdateCustomerCommandValidator();
    }

    // --- Happy Path ---
    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new UpdateCustomerCommand
        {
            CustomerId = 1,
            CustomerName = "Valid Customer",
            PhoneNumber = "0912345678",
            Email = "valid@test.com",
            Address = "Valid Address",
            Type = Shared.DTOs.Customers.CustomerType.Regular,
            Note = "Valid Note"
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- Required Fields ---
    [Fact]
    public void Validate_MissingRequiredFields_ShouldFail()
    {
        var command = new UpdateCustomerCommand
        {
            CustomerName = "",
            PhoneNumber = ""
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    // --- Phone Regex ---
    [Theory]
    [InlineData("abc")]
    [InlineData("0909@123")]
    public void Validate_InvalidPhoneFormat_ShouldFail(string invalidPhone)
    {
        var command = new UpdateCustomerCommand { PhoneNumber = invalidPhone };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
              .WithErrorMessage("Phone number contains invalid characters.");
    }

    // --- Email Validation ---
    [Fact]
    public void Validate_InvalidEmail_ShouldFail()
    {
        var command = new UpdateCustomerCommand { Email = "invalid-email" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Invalid email address format.");
    }

    [Fact]
    public void Validate_EmptyEmail_ShouldPass()
    {
        // Email là optional
        var command = new UpdateCustomerCommand
        {
            CustomerName = "Test",
            PhoneNumber = "0123",
            Type = Shared.DTOs.Customers.CustomerType.Regular,
            Email = ""
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    // --- Length Checks ---
    [Fact]
    public void Validate_StringLengthsExceeded_ShouldFail()
    {
        var longName = new string('a', 151); // Max 150
        var longAddress = new string('a', 301); // Max 300

        var command = new UpdateCustomerCommand
        {
            CustomerName = longName,
            Address = longAddress
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
        result.ShouldHaveValidationErrorFor(x => x.Address);
    }
}