using System.Reflection.Metadata;
using ClosedXML.Excel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.Modes;
using TechHaven.Application.Features.Reports.Queries.GetCommissionReport;
using TechHaven.Application.Features.Reports.Queries.GetDashboardSummary;
using TechHaven.Application.Features.Reports.Queries.GetProductsTrend;
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
  // [HttpGet("sales")]
  // [ProducesResponseType(typeof(ResponseWrapper<List<SalesReportDto>>), StatusCodes.Status200OK)]
  // public async Task<IActionResult> GetSalesReport(
  //     [FromQuery] ReportQueryDto query,
  //     CancellationToken cancellationToken)
  // {
  //   _logger.LogInformation(
  //     "Getting sales report from {StartDate} to {EndDate}, period: {PeriodType}",
  //     query.StartDate, query.EndDate, query.PeriodType);

  //   var command = new GetSalesReportQuery(
  //     query.StartDate,
  //     query.EndDate,
  //     query.PeriodType
  //   );

  //   var result = await _mediator.Send(command, cancellationToken);

  //   if (result.IsSuccess)
  //   {
  //     _logger.LogInformation(
  //       "Sales report retrieved: {Count} data points",
  //       result.Data?.Count ?? 0);
  //   }

  //   return HandleResult(result);
  // }

  // ============================================
  // 3. SALES TREND (for charts)
  // ============================================
  /// <summary>
  /// Xu hướng bán hàng (dùng để vẽ biểu đồ)
  /// </summary>
  /// <remarks>
  /// Trả về data points + tổng hợp + % tăng trưởng so với kỳ trước
  /// </remarks>
  [HttpGet("sales")]
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

  [HttpGet("products/{id}")]
  [ProducesResponseType(typeof(ResponseWrapper<ProductSalesTrendDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetProductSalesTrend(
    int id, // phải đặt trùng tên với endpoint thì APS.NET mới map được với cái trên
    [FromQuery] ReportQueryDto query,
    CancellationToken cancellationToken
  )
  {
    _logger.LogInformation(
      "Getting product {ProductId} sales trend from {StartDate} to {EndDate}",
      id, query.StartDate, query.EndDate);

    var command = new GetProductSalesTrendQuery(
      id,
      query.StartDate,
      query.EndDate,
      query.PeriodType
    );

    var result = await _mediator.Send(command, cancellationToken);

    if (result.IsSuccess)
    {
      _logger.LogInformation(
        "Product {ProductId} sales trend retrieved",
        id);
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
      [FromQuery] CommissionQueryDto request,
      CancellationToken cancellationToken = default)
  {
    _logger.LogInformation(
        "Getting commissions report for month {Month}, year {Year}",
        request.Month, request.Year);

    var query = new GetCommissionReportQuery(request.Month, request.Year);
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

  // ============================================
  // 6. EXPORT REPORT FOR SALES
  // ============================================
  /// <summary>
  /// Export báo cáo ra Excel
  /// </summary>
  [HttpPost("export/sales")]
  [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
  public async Task<IActionResult> ExportSalesReport(
      [FromBody] ReportQueryDto query,
      CancellationToken cancellationToken)
  {
    _logger.LogInformation("Exporting sales report to Excel");

    // 1. Lấy dữ liệu
    var command = new GetSalesReportQuery(
      query.StartDate,
      query.EndDate,
      query.PeriodType
    );
    var result = await _mediator.Send(command, cancellationToken);

    if (!result.IsSuccess)
    {
      return HandleResult(result);
    }

    // 2. Tạo Excel file
    using var workbook = new XLWorkbook();
    var worksheet = workbook.Worksheets.Add("Sales Report");

    // Header
    worksheet.Cell(1, 1).Value = "Period";
    worksheet.Cell(1, 2).Value = "Total Orders";
    worksheet.Cell(1, 3).Value = "Revenue";
    worksheet.Cell(1, 4).Value = "Cost";
    worksheet.Cell(1, 5).Value = "Profit";
    worksheet.Cell(1, 6).Value = "Profit Margin (%)";

    // Data
    int row = 2;
    foreach (var item in result.Data!)
    {
      worksheet.Cell(row, 1).Value = item.Period;
      worksheet.Cell(row, 2).Value = item.TotalOrders;
      worksheet.Cell(row, 3).Value = item.TotalRevenue;
      worksheet.Cell(row, 4).Value = item.TotalCost;
      worksheet.Cell(row, 5).Value = item.Profit;
      worksheet.Cell(row, 6).Value = item.ProfitMargin;
      row++;
    }

    // Auto-fit columns
    worksheet.Columns().AdjustToContents();

    // 3. Return file
    using var stream = new MemoryStream();
    workbook.SaveAs(stream);
    var content = stream.ToArray();

    return File(
      content,
      "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      $"SalesReport_{DateTime.Now:yyyyMMdd}.xlsx"
    );
  }

  // ============================================
  // 7. EXPORT REPORT FOR PRODUCTS
  // ============================================
  /// <summary>
  /// Export báo cáo ra Excel
  /// </summary>
  [HttpPost("export/products/{id}")]
  [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
  public async Task<IActionResult> ExportProductSalesReport(
    int id,
    [FromBody] ReportQueryDto query,
    CancellationToken cancellationToken)
  {
    _logger.LogInformation("Exporting product sales report to Excel");

    // 1. Lấy dữ liệu
    var command = new GetProductSalesTrendQuery(
      id,
      query.StartDate,
      query.EndDate,
      query.PeriodType
    );
    var result = await _mediator.Send(command, cancellationToken);

    if (!result.IsSuccess)
    {
      return HandleResult(result);
    }

    // 2. Tạo Excel file
    using var workbook = new XLWorkbook();
    var worksheet = workbook.Worksheets.Add("Product Sales Report");

    // Header
    worksheet.Cell(1, 1).Value = "Period";
    worksheet.Cell(1, 2).Value = "Quantity Sold";
    worksheet.Cell(1, 3).Value = "Revenue";

    // Data
    int row = 2;
    foreach (var item in result.Data!.DataPoints) // Assuming 'DataPoints' is the collection property in ProductSalesTrendDto
    {
      worksheet.Cell(row, 1).Value = item.Period;
      worksheet.Cell(row, 2).Value = item.QuantitySold;
      worksheet.Cell(row, 3).Value = item.Revenue;
      row++;
    }

    // Auto-fit columns
    worksheet.Columns().AdjustToContents();

    // 3. Return file
    using var stream = new MemoryStream();
    workbook.SaveAs(stream);
    var content = stream.ToArray();

    return File(
      content,
      "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      $"ProductSalesReport_ID-{result.Data.ProductId}_{DateTime.Now:yyyyMMdd}.xlsx"
    );
  }

  // ============================================
  // 7. EXPORT REPORT FOR PRODUCTS
  // ============================================
  /// <summary>
  /// Export báo cáo ra Excel
  /// </summary>
  [HttpPost("export/commission")]
  [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
  public async Task<IActionResult> ExportCommissionReport(
    [FromBody] CommissionQueryDto query,
    CancellationToken cancellationToken)
  {
    _logger.LogInformation("Exporting commission report to Excel");

    // 1. Lấy dữ liệu
    var command = new GetCommissionReportQuery(
      query.Month,
      query.Year
    );
    var result = await _mediator.Send(command, cancellationToken);

    if (!result.IsSuccess)
    {
      return HandleResult(result);
    }

    // 2. Tạo Excel file
    using var workbook = new XLWorkbook();
    var worksheet = workbook.Worksheets.Add("Commission Report");

    // Header
    worksheet.Cell(1, 1).Value = "No";
    worksheet.Cell(1, 2).Value = "UserFullName";
    worksheet.Cell(1, 3).Value = "RoleName";
    worksheet.Cell(1, 4).Value = "TotalSales";
    worksheet.Cell(1, 5).Value = "CommissionRate";
    worksheet.Cell(1, 6).Value = "CommissionAmount";
    worksheet.Cell(1, 7).Value = "TotalOrders";

    // Data
    int row = 2;
    foreach (var item in result.Data!)
    {
      worksheet.Cell(row, 1).Value = row - 1;
      worksheet.Cell(row, 2).Value = item.UserFullName;
      worksheet.Cell(row, 3).Value = item.RoleName;
      worksheet.Cell(row, 4).Value = item.TotalSales;
      worksheet.Cell(row, 5).Value = item.CommissionRate;
      worksheet.Cell(row, 6).Value = item.CommissionAmount;
      worksheet.Cell(row, 7).Value = item.TotalOrders;
      row++;
    }

    // Auto-fit columns
    worksheet.Columns().AdjustToContents();

    // 3. Return file
    using var stream = new MemoryStream();
    workbook.SaveAs(stream);
    var content = stream.ToArray();

    return File(
      content,
      "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      $"Commission_{DateTime.Now:yyyyMMdd}.xlsx"
    );
  }
}