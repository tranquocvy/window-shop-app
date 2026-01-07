using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Product.Commands.CreateProduct;
using TechHaven.Application.Features.Product.Commands.CreateProductsBulk;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.Products;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Products;
[ExcludeFromCodeCoverage] //Đây chỉ là file test - không cần test file này

public class CreateProductsBulkCommandHandlerTests : UnitTestBase
{
    private readonly CreateProductsBulkCommandHandler _handler;
    private readonly Mock<ILogger<CreateProductsBulkCommandHandler>> _mockLogger;
    private readonly Mock<IValidator<CreateProductCommand>> _mockValidator;

    public CreateProductsBulkCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<CreateProductsBulkCommandHandler>>();
        _mockValidator = new Mock<IValidator<CreateProductCommand>>();

        _handler = new CreateProductsBulkCommandHandler(
            MockUow.Object,
            Mapper,
            _mockLogger.Object,
            _mockValidator.Object
        );
    }

    [Fact]
    public async Task Handle_EmptyList_ShouldReturnValidationFailure()
    {
        // Arrange
        var command = new CreateProductsBulkCommand { Products = new List<ProductUpsertRequest>() };

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.Validation);
        result.ErrorMessage.Should().Contain("No products provided");
    }

    [Fact]
    public async Task Handle_ValidList_ShouldCreateAll_AndSaveOnce()
    {
        // Arrange
        var command = new CreateProductsBulkCommand
        {
            Products = new List<ProductUpsertRequest>
            {
                new() { ProductName = "Phone A", BrandName = "Brand A", SellPrice = 100, CostPrice = 80 },
                new() { ProductName = "Phone B", BrandName = "Brand B", SellPrice = 200, CostPrice = 150 }
            },
            SkipDuplicates = false,
            ValidateBeforeInsert = false
        };

        // Mock DB: Chưa có sản phẩm nào (trả về list rỗng)
        MockProductRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Product>());

        // Mock Validator: Luôn trả về Valid
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.SuccessCount.Should().Be(2);
        result.Data.FailedCount.Should().Be(0);
        result.Data.SkippedCount.Should().Be(0);

        // Quan trọng: Verify SaveChanges chỉ được gọi ĐÚNG 1 LẦN (Bulk Insert optimization)
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        // Verify AddAsync được gọi 2 lần
        MockProductRepo.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_Duplicates_WithSkipEnabled_ShouldSkipAndNotError()
    {
        // Arrange
        var command = new CreateProductsBulkCommand
        {
            Products = new List<ProductUpsertRequest>
            {
                new() { ProductName = "Existing Phone" }, // Trùng
                new() { ProductName = "New Phone" }       // Mới
            },
            SkipDuplicates = true // <--- BẬT SKIP
        };

        // Mock DB: Đã có "Existing Phone"
        var existingProducts = new List<Domain.Entities.Product>
        {
            new() { ProductName = "Existing Phone" }
        };
        MockProductRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProducts);

        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.SuccessCount.Should().Be(1); // Chỉ tạo 1 cái
        result.Data.SkippedCount.Should().Be(1); // Skip 1 cái
        result.Data.FailedCount.Should().Be(0);  // Không tính là lỗi

        // Verify chỉ gọi AddAsync 1 lần cho "New Phone"
        MockProductRepo.Verify(x => x.AddAsync(It.Is<Domain.Entities.Product>(p => p.ProductName == "New Phone"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Duplicates_WithSkipDisabled_ShouldReportError()
    {
        // Arrange
        var command = new CreateProductsBulkCommand
        {
            Products = new List<ProductUpsertRequest>
            {
                new() { ProductName = "Existing Phone" }
            },
            SkipDuplicates = false // <--- TẮT SKIP (Mặc định)
        };

        MockProductRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Product> { new() { ProductName = "Existing Phone" } });

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        // Lưu ý: Logic hiện tại trả về Success nhưng chứa list Errors (để báo cáo dòng nào lỗi)
        // hoặc Failure tùy vào cách bạn implement. Code của bạn return Result.Success(response) với FailedCount > 0.
        result.IsSuccess.Should().BeTrue();
        result.Data!.FailedCount.Should().Be(1);
        result.Data.Errors.Should().Contain(e => e.ProductName == "Existing Phone");

        // Không lưu gì cả
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidationFailure_WithValidateBeforeInsert_ShouldAbort()
    {
        // Arrange
        var command = new CreateProductsBulkCommand
        {
            Products = new List<ProductUpsertRequest>
            {
                new() { ProductName = "Invalid Phone" }
            },
            ValidateBeforeInsert = true // <--- Bật chế độ check kỹ trước khi insert
        };

        MockProductRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Product>());

        // Mock Validator: Trả về lỗi
        var validationFailure = new ValidationFailure("SellPrice", "Price invalid");
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { validationFailure }));

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Vẫn trả về object response để hiển thị lỗi
        result.Data!.FailedCount.Should().Be(1);
        result.Data.Errors.Should().Contain(e => e.ErrorMessages.Contains("SellPrice: Price invalid"));

        // QUAN TRỌNG: Do ValidateBeforeInsert = true và có lỗi, hệ thống KHÔNG được gọi SaveChanges
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}