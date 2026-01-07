using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using TechHaven.Application.Features.Product.Commands.DeleteProduct;
using TechHaven.Domain.Common;
using TechHaven.Domain.Entities;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Products;
[ExcludeFromCodeCoverage] //Đây chỉ là file test - không cần test file này

public class DeleteProductCommandHandlerTests : UnitTestBase
{
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _handler = new DeleteProductCommandHandler(MockUow.Object);
    }

    [Fact]
    public async Task Handle_ValidProduct_NoOrders_ShouldDeleteSuccessfully()
    {
        // --- ARRANGE ---
        var productId = 1;
        var product = new Domain.Entities.Product { ProductId = productId, ProductName = "Unsold Phone" };
        var command = new DeleteProductCommand  (productId );

        // 1. Mock tìm thấy sản phẩm
        MockProductRepo.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // 2. Mock check ràng buộc: KHÔNG có đơn hàng nào (return false)
        // Lưu ý: AnyAsync nhận vào một Expression predicate
        MockProductRepo.Setup(x => x.AnyAsync(
            It.IsAny<Expression<Func<Domain.Entities.Product, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeTrue();

        // Verify: Phải gọi hàm Delete và SaveChanges
        MockProductRepo.Verify(x => x.DeleteAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldReturnNotFound()
    {
        // --- ARRANGE ---
        var command = new DeleteProductCommand (999 );

        // Mock: Không tìm thấy sản phẩm (return null)
        MockProductRepo.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Product?)null);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.NotFound);
        result.ErrorMessage.Should().Contain("not found");

        // Verify: KHÔNG ĐƯỢC gọi Delete hay SaveChanges
        MockProductRepo.Verify(x => x.DeleteAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Never);
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProductHasOrders_ShouldReturnConflict()
    {
        // --- ARRANGE ---
        var productId = 1;
        var product = new Domain.Entities.Product { ProductId = productId, ProductName = "Best Seller Phone" };
        var command = new DeleteProductCommand  (productId );

        // 1. Tìm thấy sản phẩm
        MockProductRepo.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // 2. Mock check ràng buộc: CÓ đơn hàng (return true)
        MockProductRepo.Setup(x => x.AnyAsync(
            It.IsAny<Expression<Func<Domain.Entities.Product, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.Conflict); // Check Conflict
        result.ErrorMessage.Should().Contain("has order detail");

        // Verify: KHÔNG được xóa
        MockProductRepo.Verify(x => x.DeleteAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Never);
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExceptionDuringDelete_ShouldReturnInternalError()
    {
        // --- ARRANGE ---
        var productId = 1;
        var product = new Domain.Entities.Product { ProductId = productId };
        var command = new DeleteProductCommand  (productId );

        MockProductRepo.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        MockProductRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Domain.Entities.Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Giả lập lỗi DB khi SaveChanges
        MockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection lost"));

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.InternalError);
        result.ErrorMessage.Should().Contain("Failed to delete product");
    }
}