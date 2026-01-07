using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Product.Commands.UpdateProduct;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.Products;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Products;
[ExcludeFromCodeCoverage] //Đây chỉ là file test - không cần test file này

public class UpdateProductCommandHandlerTests : UnitTestBase
{
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _handler = new UpdateProductCommandHandler(MockUow.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ExistingProduct_ShouldUpdateFieldsAndReturnSuccess()
    {
        // --- ARRANGE ---
        var productId = 1;
        // Sản phẩm gốc trong DB
        var existingProduct = new Domain.Entities.Product
        {
            ProductId = productId,
            ProductName = "Old Name",
            SellPrice = 100,
            StockQuantity = 10
        };

        // Command chứa thông tin mới
        var command = new UpdateProductCommand
        {
            ProductId = productId,
            ProductName = "New Name",
            BrandName = "New Brand",
            SellPrice = 200,
            StockQuantity = 20,
            IsDraft = false
        };

        // Mock tìm thấy sản phẩm
        MockProductRepo.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Kiểm tra dữ liệu trả về đã được cập nhật chưa
        result.Data.ProductName.Should().Be("New Name");
        result.Data.SellPrice.Should().Be(200);

        // Kiểm tra Entity trong memory đã thay đổi chưa (quan trọng để verify mapper)
        existingProduct.ProductName.Should().Be("New Name");
        existingProduct.StockQuantity.Should().Be(20);
        existingProduct.UpdatedAt.Should().NotBeNull(); // Kiểm tra UpdatedAt được gán

        // Verify gọi Update và SaveChanges
        MockProductRepo.Verify(x => x.UpdateAsync(existingProduct, It.IsAny<CancellationToken>()), Times.Once);
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldReturnNotFound()
    {
        // --- ARRANGE ---
        var command = new UpdateProductCommand
        {
            ProductId = 999,
            ProductName = "Phantom Product"
        };

        // Mock không tìm thấy
        MockProductRepo.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Product?)null);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.NotFound);
        result.ErrorMessage.Should().Contain("not found");

        // Verify không lưu
        MockProductRepo.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Never);
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Exception_ShouldReturnInternalError()
    {
        // --- ARRANGE ---
        var productId = 1;
        var existingProduct = new Domain.Entities.Product { ProductId = productId };
        var command = new UpdateProductCommand { ProductId = productId };

        MockProductRepo.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Giả lập lỗi khi SaveChanges
        MockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database locked"));

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.InternalError);
        result.ErrorMessage.Should().Contain("Failed to update product");
    }
}