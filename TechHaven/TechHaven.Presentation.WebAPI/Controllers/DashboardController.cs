using MediatR;
using Microsoft.AspNetCore.Authorization;

// using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechHaven.Application.Features.Dashboard.Queries.GetDashboard;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Dashboard;

namespace TechHaven.Presentation.WebAPI.Controllers;

/// <summary>
/// Controller for dashboard overview data
/// </summary>
[Authorize]
public class DashboardController : BaseApiController
{
  private readonly IMediator _mediator;
  private readonly ILogger<DashboardController> _logger;

  public DashboardController(
    IMediator mediator,
    ILogger<DashboardController> logger)
  {
    _mediator = mediator;
    _logger = logger;
  }

  /// <summary>
  /// Get dashboard overview data
  /// </summary>
  /// <remarks>
  /// Returns comprehensive dashboard data including:
  /// - Total products count
  /// - Low stock products (quantity &lt; 5)
  /// - Top 5 best selling products
  /// - Today's order count and revenue
  /// - 3 most recent orders
  /// - Daily revenue chart for current month
  /// </remarks>
  [HttpGet]
  [ProducesResponseType(typeof(ResponseWrapper<DashboardDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> GetDashboard(
      CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Fetching dashboard overview data");

    var query = new GetDashboardQuery();
    var result = await _mediator.Send(query, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation(
        "Dashboard data retrieved successfully. Total Products: {TotalProducts}, Today Orders: {TodayOrders}",
        result.Data?.TotalProducts,
        result.Data?.TodayOrderCount);
    }
    else
    {
    _logger.LogError(
        "Failed to retrieve dashboard data: {ErrorMessage}",
        result.ErrorMessage);
    }

    return HandleResult(result);
  }
}