using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechHaven.Application.Features.Reports.Queries.GetCommissionReport;
using TechHaven.Application.Features.Reports.Queries.GetDashboardSummary;
using TechHaven.Application.Features.Reports.Queries.GetSalesReport;
using TechHaven.Application.Features.Reports.Queries.GetSalesTrend;
using TechHaven.Application.Features.Reports.Queries.GetTopSellingProducts;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Reports;

namespace TechHaven.Presentation.WebAPI.Controllers;

public class ReportController : BaseApiController
{
  private readonly IMediator _mediator;
  private readonly ILogger<ReportController> _logger;

  public ReportController(
    IMediator mediator,
    ILogger<ReportController> logger)
  {
    _mediator = mediator;
    _logger = logger;
  }

  // ============================================
  // 1. DASHBOARD SUMMARY
  // ============================================
  /// <summary>
  /// Lấy tổng quan Dashboard (today, month, inventory, top products/sellers)
  /// </summary>
  /// <remarks>
  /// API này trả về:
  /// - Doanh số hôm nay và tháng này
  /// - Tình trạng kho hàng
  /// - Top 5 sản phẩm bán chạy
  /// - Top 5 nhân viên xuất sắc
  /// - Phân bố trạng thái đơn hàng
  /// </remarks>
  [HttpGet("dashboard")]
  [ProducesResponseType(typeof(ResponseWrapper<DashboardSummaryDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Getting dashboard summary");
    var query = new GetDashboardSummaryQuery();
    var result = await _mediator.Send(query, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation("Dashboard summary retrieved successfully");
    }

    return HandleResult(result);
  }

  // ============================================
  // 2. SALES REPORT
  // ============================================
  /// <summary>
  /// Báo cáo doanh số theo khoảng thời gian
  /// </summary>
  /// <remarks>
  /// Trả về doanh thu, chi phí, lợi nhuận theo từng period (ngày/tuần/tháng/năm)
  /// </remarks>
  /// <param name="query">Query parameters</param>
  /// <param name="cancellationToken"></param>
  [HttpGet("sales")]
  [ProducesResponseType(typeof(ResponseWrapper<List<SalesReportDto>>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetSalesReport(
      [FromQuery] ReportQueryDto query,
      CancellationToken cancellationToken)
  {
    _logger.LogInformation(
      "Getting sales report from {StartDate} to {EndDate}, period: {PeriodType}",
      query.StartDate, query.EndDate, query.PeriodType);

    var command = new GetSalesReportQuery(
      query.StartDate,
      query.EndDate,
      query.PeriodType
    );

    var result = await _mediator.Send(command, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation(
        "Sales report retrieved: {Count} data points",
        result.Data?.Count ?? 0);
    }

    return HandleResult(result);
  }

  // ============================================
  // 3. SALES TREND (for charts)
  // ============================================
  /// <summary>
  /// Xu hướng bán hàng (dùng để vẽ biểu đồ)
  /// </summary>
  /// <remarks>
  /// Trả về data points + tổng hợp + % tăng trưởng so với kỳ trước
  /// </remarks>
  [HttpGet("sales/trend")]
  [ProducesResponseType(typeof(ResponseWrapper<SalesTrendDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetSalesTrend(
      [FromQuery] ReportQueryDto query,
      CancellationToken cancellationToken)
  {
    _logger.LogInformation(
        "Getting sales trend from {StartDate} to {EndDate}",
        query.StartDate, query.EndDate);

    var command = new GetSalesTrendQuery(
        query.StartDate,
        query.EndDate,
        query.PeriodType
    );

    var result = await _mediator.Send(command, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation(
          "Sales trend retrieved. Revenue growth: {RevenueGrowth}%, Profit growth: {ProfitGrowth}%",
          result.Data?.RevenueGrowth,
          result.Data?.ProfitGrowth);
    }

    return HandleResult(result);
  }

  // ============================================
  // 4. TOP SELLING PRODUCTS
  // ============================================
  /// <summary>
  /// Top sản phẩm bán chạy nhất
  /// </summary>
  /// <param name="startDate">Ngày bắt đầu</param>
  /// <param name="endDate">Ngày kết thúc</param>
  /// <param name="topCount">Số lượng sản phẩm (mặc định 10)</param>
  /// <param name="cancellationToken"></param>
  [HttpGet("products/top-selling")]
  [ProducesResponseType(typeof(ResponseWrapper<List<ProductSalesDto>>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetTopSellingProducts(
      [FromQuery] DateTime startDate,
      [FromQuery] DateTime endDate,
      [FromQuery] int topCount = 10,
      CancellationToken cancellationToken = default)
  {
    _logger.LogInformation(
        "Getting top {TopCount} selling products from {StartDate} to {EndDate}",
        topCount, startDate, endDate);

    var query = new GetTopSellingProductsQuery(startDate, endDate, topCount);
    var result = await _mediator.Send(query, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation(
          "Retrieved {Count} top selling products",
          result.Data?.Count ?? 0);
    }

    return HandleResult(result);
  }

  // ============================================
  // 5. COMMISSION REPORT
  // ============================================
  /// <summary>
  /// Báo cáo hoa hồng nhân viên
  /// </summary>
  /// <remarks>
  /// Nếu không truyền UserId, trả về tất cả nhân viên.
  /// Nếu truyền UserId, chỉ trả về nhân viên đó.
  /// </remarks>
  /// <param name="startDate">Ngày bắt đầu</param>
  /// <param name="endDate">Ngày kết thúc</param>
  /// <param name="userId">ID nhân viên (optional)</param>
  /// <param name="cancellationToken"></param>
  [HttpGet("commission")]
  [ProducesResponseType(typeof(ResponseWrapper<List<CommissionReportDto>>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetCommissionReport(
      [FromQuery] DateTime startDate,
      [FromQuery] DateTime endDate,
      [FromQuery] int? userId = null,
      CancellationToken cancellationToken = default)
  {
    _logger.LogInformation(
        "Getting commission report from {StartDate} to {EndDate}, UserId: {UserId}",
        startDate, endDate, userId?.ToString() ?? "All");

    var query = new GetCommissionReportQuery(startDate, endDate, userId);
    var result = await _mediator.Send(query, cancellationToken);

    if (result.IsSuccess)
    {
      var totalCommission = result.Data?.Sum(c => c.CommissionAmount) ?? 0;
      _logger.LogInformation(
          "Commission report retrieved: {Count} employees, Total commission: {Total:C}",
          result.Data?.Count ?? 0,
          totalCommission);
    }

    return HandleResult(result);
  }

  // // ============================================
  // // 6. EXPORT REPORT (Optional - for future)
  // // ============================================
  // /// <summary>
  // /// Export báo cáo ra Excel/PDF (TODO: implement later)
  // /// </summary>
  // [HttpPost("export")]
  // [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
  // public async Task<IActionResult> ExportReport(
  //     [FromBody] ReportQueryDto query,
  //     [FromQuery] string format = "excel") // excel or pdf
  // {
  //   _logger.LogInformation("Exporting report to {Format}", format);

  //   // TODO: Implement export logic
  //   return BadRequest(new ResponseWrapper<object>
  //   {
  //     Success = false,
  //     Message = "Export feature not implemented yet"
  //   });
  // }
}