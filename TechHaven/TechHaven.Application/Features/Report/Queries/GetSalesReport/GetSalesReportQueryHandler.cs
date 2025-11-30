using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Reports;

namespace TechHaven.Application.Features.Reports.Queries.GetSalesReport;

public class GetSalesReportQueryHandler : IQueryHandler<GetSalesReportQuery, Result<List<SalesReportDto>>> {
  private readonly IUnitOfWork _unitOfWork;

  public GetSalesReportQueryHandler(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<List<SalesReportDto>>> Handle(
    GetSalesReportQuery request,
    CancellationToken cancellationToken
  )
  {
    try
    {
      var report = await _unitOfWork.Orders.GetSalesReportAsync(
        request.StartDate,
        request.EndDate,
        (Domain.Enums.ReportPeriodType)request.PeriodType,
        cancellationToken
      );

      var reportDtos = report.Select(t => new SalesReportDto
      {
        Period = t.Period,
        TotalOrders = t.TotalOrders,
        TotalRevenue = t.TotalRevenue,
        TotalCost = t.TotalCost,
        Profit = t.Profit,
        ProfitMargin = t.ProfitMargin
      }).ToList();

      return Result<List<SalesReportDto>>.Success(reportDtos);
    }
    catch (Exception ex)
    {
      return Result<List<SalesReportDto>>.Failure(
        $"Failed to get sales report: {ex.Message}",
        ErrorType.InternalError
      );
    }
  }
}