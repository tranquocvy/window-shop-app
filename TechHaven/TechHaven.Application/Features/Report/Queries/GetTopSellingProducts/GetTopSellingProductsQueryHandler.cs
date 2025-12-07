using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.Reports;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Application.Features.Reports.Queries.GetTopSellingProducts;

public class GetTopSellingProductsQueryHandler : IQueryHandler<GetTopSellingProductsQuery, Result<List<ProductSalesDto>>> {
  private readonly IUnitOfWork _unitOfWork;

  public GetTopSellingProductsQueryHandler(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<List<ProductSalesDto>>> Handle(
    GetTopSellingProductsQuery request,
    CancellationToken cancellationToken)
  {
    try
    {
      var products = await _unitOfWork.Orders.GetTopSellingProductsAsync(
        startDate: request.StartDate,
        endDate: request.EndDate,
        count: request.TopCount,
        cancellationToken
      );

      var productDtos = products.Select(p => new ProductSalesDto
      {
        ProductId = p.ProductId,
        ProductName = p.ProductName,
        BrandName = p.BrandName,
        QuantitySold = p.TotalSold,
        TotalRevenue = p.TotalRevenue,
        TotalProfit = p.TotalRevenue - p.TotalSold
      }).ToList();

      return Result<List<ProductSalesDto>>.Success(productDtos);
    }
    catch (Exception ex)
    {
      return Result<List<ProductSalesDto>>.Failure(
        $"Failed to get products report: {ex.Message}",
        ErrorType.InternalError
      );
    }
  }
}