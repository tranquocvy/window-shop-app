using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechHaven.Application.Common.Exceptions;
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

  public ProductController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpGet]
  [ProducesResponseType(typeof(ResponseWrapper<PagingResponse<ProductDto>>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetProducts(
    [FromQuery] ProductQueryDto queryDto,
    CancellationToken cancellationToken = default)
  {
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
    var query = new GetProductByIdQuery(id);
    var result = await _mediator.Send(query, cancellationToken);

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
    return HandleResult(result);
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
      [FromBody] ProductCreateUpdateDto request,
      CancellationToken cancellationToken)
  {
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
    var command = new DeleteProductCommand(id);
    var result = await _mediator.Send(command, cancellationToken);

    return result.IsSuccess
      ? NoContent()
      : HandleResult(result);
  }
}