using AutoMapper;

using TechHaven.Domain.Interfaces;
using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Products;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Domain.Common;

namespace TechHaven.Application.Features.Product.Queries.GetProductById;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, Result<ProductDto>>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public GetProductByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
  {
    _unitOfWork = unitOfWork;
    _mapper = mapper;
  }

  public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
  {
    // Try to find product
    var product = await _unitOfWork.Products.GetByIdAsync(
      request.ProductId,
      cancellationToken
    );

    if (product == null)
    {
      return Result<ProductDto>.Failure(
        $"Product with ID {request.ProductId} not found",
        ErrorType.NotFound);
    }

    var productDto = _mapper.Map<ProductDto>(product);
    return Result<ProductDto>.Success(productDto);
  }
}