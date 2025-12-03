using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Reports;

namespace TechHaven.Application.Features.Reports.Queries.GetProductsTrend;

public class GetProductSalesTrendQueryHandler : IQueryHandler<GetProductSalesTrendQuery, Result<ProductSalesTrendDto>>
{
  private readonly IUnitOfWork _unitOfWork;

  public GetProductSalesTrendQueryHandler(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<ProductSalesTrendDto>> Handle
  (
    GetProductSalesTrendQuery request,
    CancellationToken cancellationToken = default
  )
  {
    try
    {
      var dataPoints = await _unitOfWork.Orders.GetProductSalesDataPointAsync
      (
        request.ProductId,
        request.StartDate,
        request.EndDate,
        (Domain.Enums.ReportPeriodType)request.PeriodType,
        cancellationToken
      );

      var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);

      var dataPointsDto = dataPoints.Select(d => new ProductSalesDataPointDto
      {
        Period = d.Period,
        QuantitySold = d.QuantitySold,
        Revenue = d.Revenue
      }).ToList();

      var result = new ProductSalesTrendDto
      {
        ProductId = request.ProductId,
        ProductName = product?.ProductName ?? string.Empty,
        TotalQuantitySold = dataPoints.Sum(d => d.QuantitySold),
        TotalRevenue = dataPoints.Sum(d => d.Revenue),
        DataPoints = dataPointsDto
      };

      return Result<ProductSalesTrendDto>.Success(result);
    }
    catch (Exception ex)
    {
      return Result<ProductSalesTrendDto>.Failure(
        $"Failed to get product sales trend: {ex.Message}",
        ErrorType.InternalError
      );
      throw;
    }
  }
}