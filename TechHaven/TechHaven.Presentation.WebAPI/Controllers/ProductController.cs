using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Products;
using TechHaven.Application.Features.Product.Queries.GetProducts;
using TechHaven.Application.Features.Product.Queries.GetProductById;
using TechHaven.Application.Features.Product.Commands.CreateProduct;
using TechHaven.Application.Features.Product.Commands.UpdateProduct;
using TechHaven.Application.Features.Product.Commands.DeleteProduct;

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
  [ProducesResponseType(typeof(PagingResponse<ProductDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<PagingResponse<ProductDto>>> GetProducts(
    [FromQuery] string? searchTerm = null,
    [FromQuery] bool? isDraft = null,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? sortBy = null,
    [FromQuery] bool sortDescending = false,
    CancellationToken cancellationToken = default)
  {
    var query = new GetProductsQuery
    {
      SearchTerm = searchTerm,
      IsDraft = isDraft,
      PageNumber = pageNumber,
      PageSize = pageSize,
      SortBy = sortBy,
      SortDescending = sortDescending
    };

    var result = await _mediator.Send(query, cancellationToken);
    return Ok(result);
  }

  /// <summary>
  /// Get a single product by ID
  /// </summary>
  [HttpGet("{id}")]
  [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ProductDto>> GetProductById(
      int id,
      CancellationToken cancellationToken)
  {
    var query = new GetProductByIdQuery(id);
    var result = await _mediator.Send(query, cancellationToken);
    return Ok(result);
  }

  /// <summary>
  /// Create a new product
  /// </summary>
  [HttpPost]
  [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<ProductDto>> CreateProduct(
      [FromBody] CreateProductCommand command,
      CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(command, cancellationToken);
    return CreatedAtAction(
        nameof(GetProductById),
        new { id = result.ProductId },
        result);
  }

  /// <summary>
  /// Update an existing product
  /// </summary>
  [HttpPut("{id}")]
  [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<ProductDto>> UpdateProduct(
      int id,
      [FromBody] UpdateProductCommand command,
      CancellationToken cancellationToken)
  {
    // Ensure ID in route matches ID in body
    if (id != command.productDto.ProductId)
    {
      return BadRequest("Product ID in route does not match ID in request body.");
    }

    var result = await _mediator.Send(command, cancellationToken);
    return Ok(result);
  }

  /// <summary>
  /// Delete a product
  /// </summary>
  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> DeleteProduct(
      int id,
      CancellationToken cancellationToken)
  {
    var command = new DeleteProductCommand(id);
    await _mediator.Send(command, cancellationToken);
    return NoContent();
  }
}