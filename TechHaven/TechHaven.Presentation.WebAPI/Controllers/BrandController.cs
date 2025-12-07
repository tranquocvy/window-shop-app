using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechHaven.Application.Features.Brand.Queries.GetBrandsQuery;
using TechHaven.Shared.DTOs.Brands;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Presentation.WebAPI.Controllers;

public class BrandController : BaseApiController
{
  private readonly IMediator _mediator;
  private readonly ILogger<BrandController> _logger;

  public BrandController(
    IMediator mediator,
    ILogger<BrandController> logger
  )
  {
    _mediator = mediator;
    _logger = logger;
  }

  [HttpGet]
  [ProducesResponseType(typeof(ResponseWrapper<PagingResponse<BrandDto>>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetBrands(
    [FromQuery] BrandQueryDto request,
    CancellationToken cancellationToken
  )
  {
    _logger.LogInformation(
      "Getting brands with SearchTerm: {SearchTerm}",
      request.SearchTerm
    );

    var query = new GetBrandsQuery(
      request.SearchTerm ?? string.Empty,
      request.InStockOnly,
      request.SortBy,
      request.SortDescending
    );

    var result = await _mediator.Send(query, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation(
        "Retrieved {Count} brands",
        result.Data?.Count()
      );
    }

    return HandleResult(result);
  }
}