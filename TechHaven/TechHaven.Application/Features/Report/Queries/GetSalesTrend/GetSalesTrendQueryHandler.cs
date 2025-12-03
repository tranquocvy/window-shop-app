using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Reports;

namespace TechHaven.Application.Features.Reports.Queries.GetSalesTrend;

public class GetSalesTrendQueryHandler : IQueryHandler<GetSalesTrendQuery, Result<SalesTrendDto>>
{
  private readonly IUnitOfWork _unitOfWork;

  public GetSalesTrendQueryHandler(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<SalesTrendDto>> Handle(
    GetSalesTrendQuery request,
    CancellationToken cancellationToken
  )
  {
    try
    {
      var dataPoints = await _unitOfWork.Orders.GetSalesReportAsync(
        request.StartDate,
        request.EndDate,
        (Domain.Enums.ReportPeriodType)request.PeriodType,
        cancellationToken
      );

      var dataPointDtos = dataPoints.Select(d => new SalesReportDto
      {
        Period = d.Period,
        TotalOrders = d.TotalOrders,
        TotalRevenue = d.TotalRevenue,
        TotalCost = d.TotalCost,
        Profit = d.Profit,
        ProfitMargin = d.ProfitMargin
      }).ToList();

      // Calculator summary
      var summary = new SalesReportDto
      {
        Period = $"{request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}",
        TotalOrders = dataPoints.Sum(d => d.TotalOrders),
        TotalRevenue = dataPoints.Sum(d => d.TotalRevenue),
        TotalCost = dataPoints.Sum(d => d.TotalCost),
        Profit = dataPoints.Sum(d => d.Profit),
        ProfitMargin = CalculateProfitMargin(
          dataPoints.Sum(d => d.TotalRevenue),
          dataPoints.Sum(d => d.TotalCost)
        )
      };

      // Calculate growth (so với kỳ trước)
      var previousPeriodStart = request.StartDate.AddDays(-(request.EndDate - request.StartDate).Days - 1);
      var previousPeriodEnd = request.StartDate.AddDays(-1);

      var previousData = await _unitOfWork.Orders.GetSalesReportAsync(
          previousPeriodStart,
          previousPeriodEnd,
          (Domain.Enums.ReportPeriodType)request.PeriodType,
          cancellationToken
      );

      var previousRevenue = previousData.Sum(d => d.TotalRevenue);
      var previousProfit = previousData.Sum(d => d.Profit);

      var result = new SalesTrendDto
      {
        DataPoints = dataPointDtos,
        Summary = summary,
        RevenueGrowth = CalculateGrowth(previousRevenue, summary.TotalRevenue),
        ProfitGrowth = CalculateGrowth(previousProfit, summary.Profit)
      };

      return Result<SalesTrendDto>.Success(result);
    }
    catch (Exception ex)
    {
      return Result<SalesTrendDto>.Failure(
        $"Failed to get sales trend: {ex.Message}",
        ErrorType.InternalError
      );
    }
  }

  private decimal CalculateGrowth(decimal previousValue, decimal currentValue)
  {
    if (previousValue == 0) return currentValue > 0 ? 100 : 0;
    return Math.Round(((currentValue - previousValue) / previousValue) * 100, 2);
  }

  private decimal CalculateProfitMargin(decimal revenue, decimal cost)
  {
    if (revenue == 0) return 0;
    return Math.Round(((revenue - cost) / revenue) * 100, 2);
  }
}