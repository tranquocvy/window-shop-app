using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Products;
using TechHaven.Application.Features.Product.Queries.GetProducts;
using TechHaven.Application.Features.Product.Queries.GetProductById;
using TechHaven.Application.Features.Product.Commands.CreateProduct;
using TechHaven.Application.Features.Product.Commands.UpdateProduct;
using TechHaven.Application.Features.Product.Commands.DeleteProduct;
using TechHaven.Application.Common.Exceptions;

namespace TechHaven.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
  private readonly IMediator _mediator;

  public ProductController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpGet]
  [ProducesResponseType(typeof(ResponseWrapper<PagingResponse<ProductDto>>), StatusCodes.Status200OK)]
  public async Task<ActionResult<ResponseWrapper<PagingResponse<ProductDto>>>> GetProducts(
    [FromQuery] ProductListQueryDto queryDto,
    CancellationToken cancellationToken = default)
  {
    var query = new GetProductsQuery
    {
      SearchTerm = queryDto.SearchTerm,
      IsDraft = queryDto.IsDraft,
      PageNumber = queryDto.PageNumber,
      PageSize = queryDto.PageSize,
      SortBy = queryDto.Sorting?.SortBy,
      SortDescending = queryDto.Sorting?.Desc ?? false
    };

    var result = await _mediator.Send(query, cancellationToken);
    return Ok(new ResponseWrapper<PagingResponse<ProductDto>>
    {
      Success = true,
      Message = "Products retrieved successfully.",
      Data = result
    });
  }

  /// <summary>
  /// Get a single product by ID
  /// </summary>
  [HttpGet("{id}")]
  [ProducesResponseType(typeof(ResponseWrapper<ProductDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ResponseWrapper<ProductDto>>> GetProductById(
      int id,
      CancellationToken cancellationToken)
  {
    try
    {
      var query = new GetProductByIdQuery(id);
      var result = await _mediator.Send(query, cancellationToken);
      return Ok(new ResponseWrapper<ProductDto>
      {
        Success = true,
        Message = "Product retrieved successfully.",
        Data = result
      });
    }
    catch (NotFoundException ex)
    {
      return NotFound(new ResponseWrapper<object>
      {
        Success = false,
        Message = ex.Message
      });
    }
  }

  /// <summary>
  /// Create a new product
  /// </summary>
  [HttpPost]
  [ProducesResponseType(typeof(ResponseWrapper<ProductDto>), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<ResponseWrapper<ProductDto>>> CreateProduct(
      [FromBody] ProductUpsertRequest request,
      CancellationToken cancellationToken)
  {
    try
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
      return CreatedAtAction(
        nameof(GetProductById),
        new { id = result.ProductId },
        new ResponseWrapper<ProductDto>
        {
          Success = true,
          Message = "Product created successfully.",
          Data = result
        });
    }
    catch (Exception ex)
    {
      return BadRequest(new ResponseWrapper<object>
      {
        Success = false,
        Message = "Failed to create product.",
        Errors = new List<string> { ex.Message }
      });
    }
  }

  /// <summary>
  /// Update an existing product
  /// </summary>
  [HttpPut("{id}")]
  [ProducesResponseType(typeof(ResponseWrapper<ProductDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ResponseWrapper<ProductDto>>> UpdateProduct(
      int id,
      [FromBody] ProductUpsertRequest request,
      CancellationToken cancellationToken)
  {
    try
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
      return Ok(new ResponseWrapper<ProductDto>
      {
        Success = true,
        Message = "Product updated successfully.",
        Data = result
      });
    }
    catch (NotFoundException ex)
    {
      return NotFound(new ResponseWrapper<object>
      {
        Success = false,
        Message = ex.Message
      });
    }
  }

  /// <summary>
  /// Delete a product
  /// </summary>
  [HttpDelete("{id}")]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ResponseWrapper<object>>> DeleteProduct(
      int id,
      CancellationToken cancellationToken)
  {
    try
    {
      var command = new DeleteProductCommand(id);
      await _mediator.Send(command, cancellationToken);
      return NoContent();
    }
    catch (NotFoundException ex)
    {
      return NotFound(new ResponseWrapper<object>
      {
        Success = false,
        Message = ex.Message
      });
    }
  }
}