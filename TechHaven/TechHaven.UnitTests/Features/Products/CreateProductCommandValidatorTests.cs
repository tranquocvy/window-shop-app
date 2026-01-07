using FluentValidation.TestHelper;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Product.Commands.CreateProduct;
using Xunit;

namespace TechHaven.UnitTests.Features.Products;
[ExcludeFromCodeCoverage] //Đây chỉ là file test - không cần test file này

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator;

    public CreateProductCommandValidatorTests()
    {
        _validator = new CreateProductCommandValidator();
    }

    // --- TEST CÁC LUẬT CHUNG (COMMON RULES) ---

    [Fact]
    public void Should_Have_Error_When_ProductName_Is_Empty()
    {
        var command = new CreateProductCommand { ProductName = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }

    [Fact]
    public void Should_Have_Error_When_Prices_Are_Negative()
    {
        var command = new CreateProductCommand
        {
            CostPrice = -10,
            SellPrice = -1,
            StockQuantity = -5
        };
        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CostPrice);
        result.ShouldHaveValidationErrorFor(x => x.SellPrice);
        result.ShouldHaveValidationErrorFor(x => x.StockQuantity);
    }

    // --- TEST LOGIC DRAFT (LUẬT LỎNG) ---

    [Fact]
    public void DraftMode_Should_Pass_Even_If_BrandName_Missing_Or_Price_Invalid()
    {
        var command = new CreateProductCommand
        {
            IsDraft = true, // Đang nháp
            ProductName = "Draft Phone",
            BrandName = "", // Thiếu Brand (vẫn OK)
            CostPrice = 1000,
            SellPrice = 500, // Bán lỗ (vẫn OK vì đang nháp)
            StockQuantity = 0
        };

        var result = _validator.TestValidate(command);

        // Assert KHÔNG có lỗi
        result.ShouldNotHaveValidationErrorFor(x => x.BrandName);
        result.ShouldNotHaveValidationErrorFor(x => x.SellPrice);
    }

    // --- TEST LOGIC PUBLISH (LUẬT CHẶT) ---

    [Fact]
    public void PublishMode_Should_Fail_If_BrandName_Missing()
    {
        var command = new CreateProductCommand
        {
            IsDraft = false, // Xuất bản
            ProductName = "Real Phone",
            BrandName = "" // Thiếu Brand -> Lỗi
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.BrandName)
              .WithErrorMessage("Brand name is required for published products.");
    }

    [Fact]
    public void PublishMode_Should_Fail_If_SellPrice_Less_Than_CostPrice()
    {
        var command = new CreateProductCommand
        {
            IsDraft = false, // Xuất bản
            ProductName = "Loss Phone",
            BrandName = "Test",
            CostPrice = 1000,
            SellPrice = 900 // Bán lỗ -> Lỗi
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SellPrice)
              .WithErrorMessage("Sell price must be greater than or equal to cost price.");
    }

    [Fact]
    public void PublishMode_Should_Pass_If_Data_Is_Valid()
    {
        var command = new CreateProductCommand
        {
            IsDraft = false,
            ProductName = "Valid Phone",
            BrandName = "Samsung",
            CostPrice = 1000,
            SellPrice = 1200, // Có lời
            StockQuantity = 10
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}