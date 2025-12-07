using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Brands;

namespace TechHaven.Application.Features.Brand.Queries.GetBrandsQuery;

public class GetBrandsQueryHandler : IQueryHandler<GetBrandsQuery, Result<List<BrandDto>>>
{
  private readonly IUnitOfWork _unitOfWork;

  public GetBrandsQueryHandler(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<List<BrandDto>>> Handle(
    GetBrandsQuery request,
    CancellationToken cancellationToken
  )
  {
    try
    {
      // 1. Gọi repository để lấy danh sách brand bằng cách groupby và select về tên
      var brandList = await _unitOfWork.Products.GetBrandsAsync(
        request.searchTerm,
        request.inStockOnly,
        request.sortBy,
        request.sortDescending,
        cancellationToken
      );

      // 2. Map từ list trên sang Dto
      var response = brandList.Select(b => new BrandDto
      {
        BrandName = b.BrandName,
        ProductCount = b.ProductCount,
        MinPrice = b.MinPrice,
        MaxPrice = b.MaxPrice,
        TotalStock = b.TotalStock
      }).ToList();

      // 3. Trả về
      return Result<List<BrandDto>>.Success(response);
    }
    catch (Exception ex)
    {
      return Result<List<BrandDto>>.Failure(
        $"Failed to get brands: {ex.Message}",
        ErrorType.InternalError);
    }
  }
}