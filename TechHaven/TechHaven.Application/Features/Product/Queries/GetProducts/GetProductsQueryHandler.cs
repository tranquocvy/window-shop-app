using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Products;
using TechHaven.Domain.Interfaces;
using AutoMapper;
using TechHaven.Domain.Common;

namespace TechHaven.Application.Features.Product.Queries.GetProducts;

public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, Result<PagingResponse<ProductDto>>>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public GetProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
  {
    _unitOfWork = unitOfWork;
    _mapper = mapper;
  }

  public async Task<Result<PagingResponse<ProductDto>>> Handle(
    GetProductsQuery request,
    CancellationToken cancellationToken)
  {
    try
    {
      // 2. Gọi repository để lấy dữ liệu với phân trang
      var (products, totalCount) = await _unitOfWork.Products.SearchWithPaginationAsync(request.criteria, cancellationToken);

      // 3. Map danh sách sản phẩm từ entity sang DTO
      var productDtos = _mapper.Map<List<ProductDto>>(products);

      // 4. Tạo đối tượng PagingResponse
      var response = new PagingResponse<ProductDto>
      {
        Items = productDtos,
        TotalCount = totalCount,
        PageNumber = request.criteria.PageNumber,
        PageSize = request.criteria.PageSize
      };

      // 5. Trả về kết quả thành công
      return Result<PagingResponse<ProductDto>>.Success(response);
    }
    catch (Exception ex)
    {
      return Result<PagingResponse<ProductDto>>.Failure(
        $"Failed to get products: {ex.Message}",
        ErrorType.InternalError);
    }
  }
}