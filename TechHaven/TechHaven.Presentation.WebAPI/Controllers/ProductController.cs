using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechHaven.Application.Features.Product.Commands.CreateProduct;
using TechHaven.Application.Features.Product.Commands.CreateProductsBulk;
using TechHaven.Application.Features.Product.Commands.DeleteProduct;
using TechHaven.Application.Features.Product.Commands.UpdateProduct;
using TechHaven.Application.Features.Product.Queries.GetProductById;
using TechHaven.Application.Features.Product.Queries.GetProducts;
using TechHaven.Domain.SearchCriteria;
using TechHaven.Infrastructure.Authorization;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WebAPI.Controllers;

[Authorize(Policy = AuthorizationPolicies.ManageProducts)]
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
    [FromQuery] ProductListQueryDto queryDto,
    CancellationToken cancellationToken = default)
  {
    _logger.LogInformation(
      "Getting products with SearchTerm: {SearchTerm}, IsDraft: {IsDraft}, Page: {PageNumber}/{PageSize}",
      queryDto.SearchTerm, queryDto.IsDraft, queryDto.PageNumber, queryDto.PageSize);

    // Map CustomerQueryDto -> CustomerSearchCriteria
    var criteria = new ProductSearchCriteria
    {
      SearchTerm = queryDto.SearchTerm,
      IsDraft = queryDto.IsDraft,
      FromPrice = queryDto.FromPrice,
      ToPrice = queryDto.ToPrice,
      Brand = queryDto.Brand,
      Status = (Domain.SearchCriteria.ProductStatus)(queryDto.Status ?? default),
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
    [FromBody] ProductUpsertRequest request,
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
  [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
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

  [HttpPost("bulk")]
  [ProducesResponseType(typeof(ResponseWrapper<ProductBulkCreateResponseDto>), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ResponseWrapper<ProductBulkCreateResponseDto>), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> CreateProductBulk(
    [FromBody] ProductBulkCreateRequestDto request,
    CancellationToken cancellationToken
  )
  {
    _logger.LogInformation(
      "Bulk product import requested: {Count} products",
      request.Products.Count);

    var command = new CreateProductsBulkCommand()
    {
      Products = request.Products,
      SkipDuplicates = request.SkipDuplicates,
      ValidateBeforeInsert = request.ValidateBeforeInsert
    };

    var result = await _mediator.Send(command, cancellationToken);

    if (result.IsSuccess && result.Data != null)
    {
      // Nếu có lỗi validation, trả về BadRequest
      if (result.Data.FailedCount > 0 && result.Data.SuccessCount == 0)
      {
        _logger.LogWarning(
          "Bulk import validation failed: {Failed} errors",
          result.Data.FailedCount);

        return BadRequest(new ResponseWrapper<ProductBulkCreateResponseDto>
        {
          Success = false,
          Message = "Bulk import validation failed",
          Data = result.Data,
          Errors = result.Data.Errors.SelectMany(e => e.ErrorMessages).ToList()
        });
      }

      _logger.LogInformation(
        "Bulk import completed: {Success}/{Total} products created",
        result.Data.SuccessCount, result.Data.TotalProducts);

      return StatusCode(StatusCodes.Status201Created, new ResponseWrapper<ProductBulkCreateResponseDto>
      {
        Success = true,
        Message = $"Successfully imported {result.Data.SuccessCount} out of {result.Data.TotalProducts} products",
        Data = result.Data
      });
    }
    else if (result.Data != null)
    {
      // Validation failed but we have partial results
      _logger.LogWarning(
        "Bulk import validation failed: {Failed} errors",
        result.Data.FailedCount);

      return BadRequest(new ResponseWrapper<ProductBulkCreateResponseDto>
      {
        Success = false,
        Message = result.ErrorMessage ?? "Bulk import validation failed",
        Data = result.Data,
        Errors = result.Data.Errors.SelectMany(e => e.ErrorMessages).ToList()
      });
    }

    return HandleResult(result);
  }
}