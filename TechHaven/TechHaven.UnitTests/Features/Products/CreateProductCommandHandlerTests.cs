using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using TechHaven.Application.Features.Product.Commands.CreateProduct;
using TechHaven.Domain.Common;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Products; // Chứa CreateProductCommand nếu bạn để chung namespace DTO, hoặc namespace lệnh
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Products;
[ExcludeFromCodeCoverage] //Đây chỉ là file test - không cần test file này

public class CreateProductCommandHandlerTests : UnitTestBase
{
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _handler = new CreateProductCommandHandler(MockUow.Object, Mapper);
    }

    [Fact] // Happy Path
    public async Task Handle_ValidNewProduct_ShouldCreateSuccessfully()
    {
        // --- ARRANGE ---
        // 1. Tạo Command hợp lệ
        var command = new CreateProductCommand
        {
            ProductName = "iPhone 16 Pro Max",
            BrandName = "Apple",
            CostPrice = 20000000,
            SellPrice = 30000000,
            StockQuantity = 50,
            IsDraft = false
        };

        // 2. Setup Mock: Chưa có sản phẩm nào trùng tên trong DB
        // Lưu ý: FirstOrDefaultAsync nhận vào Expression
        MockProductRepo.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Domain.Entities.Product, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Product?)null);

        // Setup AddAsync để trả về chính entity đó (giả lập EF Core)
        MockProductRepo.Setup(x => x.AddAsync(
            It.IsAny<Domain.Entities.Product>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Product p, CancellationToken ct) => p);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.ProductName.Should().Be(command.ProductName);

        // Verify SaveChanges được gọi
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact] // Business Rule: Duplicate Name
    public async Task Handle_DuplicateProductName_ShouldReturnConflict()
    {
        // --- ARRANGE ---
        var command = new CreateProductCommand { ProductName = "Existing Phone" };

        // Setup Mock: Tìm thấy 1 sản phẩm đã tồn tại
        var existingProduct = new Domain.Entities.Product { ProductId = 1, ProductName = "Existing Phone" };

        MockProductRepo.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Domain.Entities.Product, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.Conflict); // Kiểm tra đúng loại lỗi
        result.ErrorMessage.Should().Contain("already exists");

        // Verify KHÔNG gọi SaveChanges
        MockProductRepo.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Never);
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact] // Internal Error Scenario
    public async Task Handle_RepositoryException_ShouldReturnInternalError()
    {
        // --- ARRANGE ---
        var command = new CreateProductCommand { ProductName = "Error Phone" };

        // Setup Mock: Quăng Exception khi truy vấn DB
        MockProductRepo.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Domain.Entities.Product, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.InternalError);
        result.ErrorMessage.Should().Contain("Failed to create product");
    }
}