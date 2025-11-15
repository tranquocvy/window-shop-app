using AutoMapper;

using TechHaven.Domain.Interfaces;
using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Products;
using TechHaven.Application.Common.Exceptions;


namespace TechHaven.Application.Features.Product.Queries.GetProductById;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDto>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public GetProductByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
  {
    _unitOfWork = unitOfWork;
    _mapper = mapper;
  }

  public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
  {
    // Try to find product
    var product = await _unitOfWork.Products.GetByIdAsync(
      request.ProductId,
      cancellationToken
    );

    if (product == null)
    {
      throw new NotFoundException(nameof(Domain.Entities.Product), request.ProductId);
    }

    // Map to DTO
    return _mapper.Map<ProductDto>(product);
  }
}