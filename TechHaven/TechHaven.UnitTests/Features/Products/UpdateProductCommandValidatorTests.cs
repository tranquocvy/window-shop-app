using FluentValidation.TestHelper;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Product.Commands.UpdateProduct;
using Xunit;

namespace TechHaven.UnitTests.Features.Products;
[ExcludeFromCodeCoverage] //Đây chỉ là file test - không cần test file này

public class UpdateProductCommandValidatorTests
{
    private readonly UpdateProductCommandValidator _validator;

    public UpdateProductCommandValidatorTests()
    {
        _validator = new UpdateProductCommandValidator();
    }

    // --- Happy Path ---
    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new UpdateProductCommand
        {
            ProductId = 1,
            ProductName = "Valid Product",
            BrandName = "Valid Brand",
            SellPrice = 100,
            StockQuantity = 10,
            Color = "Red",
            ScreenSize = 6.1m,
            StorageCapacity = 128,
            BatteryCapacity = 3000,
            Processor = "A15"
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- Validation Failures ---

    [Fact]
    public void Validate_InvalidIdAndName_ShouldFail()
    {
        var command = new UpdateProductCommand
        {
            ProductId = 0, // Invalid
            ProductName = "" // Invalid
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ProductId);
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }

    [Fact]
    public void Validate_NegativeValues_ShouldFail()
    {
        var command = new UpdateProductCommand
        {
            SellPrice = -1,
            StockQuantity = -10,
            ScreenSize = 0, // Invalid (>0)
            StorageCapacity = 0,
            BatteryCapacity = -100
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SellPrice);
        result.ShouldHaveValidationErrorFor(x => x.StockQuantity);
        result.ShouldHaveValidationErrorFor(x => x.ScreenSize);
        result.ShouldHaveValidationErrorFor(x => x.StorageCapacity);
        result.ShouldHaveValidationErrorFor(x => x.BatteryCapacity);
    }

    [Fact]
    public void Validate_StringLengths_ShouldFail_WhenExceeded()
    {
        // Tạo chuỗi dài hơn mức cho phép
        var longString201 = new string('a', 201);
        var longString101 = new string('a', 101);
        var longString51 = new string('a', 51);

        var command = new UpdateProductCommand
        {
            ProductName = longString201, // Max 200
            BrandName = longString101,   // Max 100
            Processor = longString101,   // Max 100
            Color = longString51         // Max 50
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ProductName);
        result.ShouldHaveValidationErrorFor(x => x.BrandName);
        result.ShouldHaveValidationErrorFor(x => x.Processor);
        result.ShouldHaveValidationErrorFor(x => x.Color);
    }

    // --- Conditional Validation (When) ---

    [Fact]
    public void Validate_OptionalFields_ShouldPass_WhenNull()
    {
        // Các trường optional như Color, Processor, ScreenSize để null -> vẫn pass
        var command = new UpdateProductCommand
        {
            ProductId = 1,
            ProductName = "Product",
            BrandName = "Brand",
            Color = null,
            Processor = null,
            ScreenSize = null
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}