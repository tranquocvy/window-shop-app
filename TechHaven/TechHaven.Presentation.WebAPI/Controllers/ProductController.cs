using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechHaven.Application.Features.Product.Commands.CreateProduct;
using TechHaven.Application.Features.Product.Commands.DeleteProduct;
using TechHaven.Application.Features.Product.Commands.UpdateProduct;
using TechHaven.Application.Features.Product.Queries.GetProductById;
using TechHaven.Application.Features.Product.Queries.GetProducts;
using TechHaven.Domain.SearchCriteria;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WebAPI.Controllers;

public class ProductController : BaseApiController
{
  private readonly IMediator _mediator;
  private readonly ILogger<ProductController> _logger;

  public ProductController(IMediator mediator, ILogger<ProductController> logger)
  {
    _mediator = mediator;
    _logger = logger;
  }

  [HttpGet]
  [ProducesResponseType(typeof(ResponseWrapper<PagingResponse<ProductDto>>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetProducts(
    [FromQuery] ProductQueryDto queryDto,
    CancellationToken cancellationToken = default)
  {
    _logger.LogInformation(
      "Getting products with SearchTerm: {SearchTerm}, IsDraft: {IsDraft}, Page: {PageNumber}/{PageSize}",
      queryDto.SearchTerm, queryDto.IsDraft, queryDto.PageNumber, queryDto.PageSize);

    // Map CustomerQueryDto -> CustomerSearchCriteria
    var criteria = new ProductSearchCriteria
    {
      IsDraft = queryDto.IsDraft,
      SearchTerm = queryDto.SearchTerm,
      PageNumber = queryDto.PageNumber,
      PageSize = queryDto.PageSize,
      SortBy = queryDto.Sorting?.SortBy,
      SortDescending = queryDto.Sorting?.Desc ?? false,
    };

    var query = new GetProductsQuery(criteria);
    var result = await _mediator.Send(query, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation(
        "Retrieved {Count} products (Total: {TotalCount})",
        result.Data?.Items.Count, result.Data?.TotalCount);
    }

    return HandleResult(result);
  }

  /// <summary>
  /// Get a single product by ID
  /// </summary>
  [HttpGet("{id}")]
  [ProducesResponseType(typeof(ResponseWrapper<ProductDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
  public async Task<IActionResult> GetProductById(
      int id,
      CancellationToken cancellationToken)
  {
    _logger.LogInformation("Getting product with ID: {ProductId}", id);

    var query = new GetProductByIdQuery(id);
    var result = await _mediator.Send(query, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation("Product found: {ProductName}", result.Data?.ProductName);
    }
    else
    {
      _logger.LogWarning("Product not found: {ProductId}", id);
    }

    return HandleResult(result);
  }

  /// <summary>
  /// Create a new product
  /// </summary>
  [HttpPost]
  [ProducesResponseType(typeof(ResponseWrapper<ProductDto>), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> CreateProduct(
      [FromBody] ProductCreateUpdateDto request,
      CancellationToken cancellationToken)
  {
    _logger.LogInformation(
      "Creating product: {ProductName} - Brand: {BrandName}",
      request.ProductName, request.BrandName
    );
    // Map DTO to Command
    var command = new CreateProductCommand
    {
      ProductName = request.ProductName,
      BrandName = request.BrandName,
      Color = request.Color,
      StorageCapacity = request.StorageCapacity,
      Processor = request.Processor,
      ScreenSize = request.ScreenSize,
      BatteryCapacity = request.BatteryCapacity,
      ImageUrl = request.ImageUrl,
      ImageGalleryJson = request.ImageGalleryJson,
      CostPrice = request.CostPrice,
      SellPrice = request.SellPrice,
      StockQuantity = request.StockQuantity,
      Description = request.Description,
      IsDraft = request.IsDraft
    };

    var result = await _mediator.Send(command, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation(
        "Product created successfully: ID {ProductId}, Name: {ProductName}",
        result.Data?.ProductId, result.Data?.ProductName);
    }
    else
    {
      _logger.LogWarning(
        "Failed to create product: {ProductName}. Error: {ErrorMessage}",
        request.ProductName, result.ErrorMessage);
    }

    return HandleResult(
      result,
      nameof(GetProductById),
      new { id = result.Data?.ProductId }
    );
  }

  /// <summary>
  /// Update an existing product
  /// </summary>
  [HttpPut("{id}")]
  [ProducesResponseType(typeof(ResponseWrapper<ProductDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
  public async Task<IActionResult> UpdateProduct(
      int id,
      [FromBody] ProductUpsertRequest request,
      CancellationToken cancellationToken)
  {
    _logger.LogInformation(
      "Updating product ID: {ProductId} - New name: {ProductName}",
      id, request.ProductName);

    // Map DTO to Command with ID from route
    var command = new UpdateProductCommand
    {
      ProductId = id,
      ProductName = request.ProductName,
      BrandName = request.BrandName,
      Color = request.Color,
      StorageCapacity = request.StorageCapacity,
      Processor = request.Processor,
      ScreenSize = request.ScreenSize,
      BatteryCapacity = request.BatteryCapacity,
      ImageUrl = request.ImageUrl,
      ImageGalleryJson = request.ImageGalleryJson,
      CostPrice = request.CostPrice,
      SellPrice = request.SellPrice,
      StockQuantity = request.StockQuantity,
      Description = request.Description,
      IsDraft = request.IsDraft
    };

    var result = await _mediator.Send(command, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation("Product updated successfully: ID {ProductId}", id);
    }

    return HandleResult(result);
  }

  /// <summary>
  /// Delete a product
  /// </summary>
  [HttpDelete("{id}")]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
  public async Task<IActionResult> DeleteProduct(
      int id,
      CancellationToken cancellationToken)
  {
    _logger.LogInformation("Deleting product ID: {ProductId}", id);

    var command = new DeleteProductCommand(id);
    var result = await _mediator.Send(command, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation("Product deleted successfully: ID {ProductId}", id);
    }

    return result.IsSuccess
      ? NoContent()
      : HandleResult(result);
  }
}