using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Products;
using AutoMapper;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Application.Features.Product.Commands.CreateProduct;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, ProductDto>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public CreateProductCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper)
  {
    _unitOfWork = unitOfWork;
    _mapper = mapper;
  }

  public async Task<ProductDto> Handle(
    CreateProductCommand request,
    CancellationToken cancellationToken)
  {
    // map command (dto) to entity
    var product = _mapper.Map<Domain.Entities.Product>(request.productDto);

    // set timestamps
    product.CreatedAt = DateTime.Now;
    product.UpdatedAt = null;

    // add to repository
    await _unitOfWork.Products.AddAsync(product, cancellationToken);

    // save changes
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return _mapper.Map<ProductDto>(product);
  }
}