using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Products;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.Common;
using AutoMapper;

namespace TechHaven.Application.Features.Product.Commands.CreateProduct;

public class CreateProductCommandHandler 
    : ICommandHandler<CreateProductCommand, Result<ProductDto>>
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

  public async Task<Result<ProductDto>> Handle(
    CreateProductCommand request,
    CancellationToken cancellationToken)
  {
    try
    {
      var existingProduct = await _unitOfWork.Products
        .FirstOrDefaultAsync(
          p => p.ProductName == request.ProductName,
          cancellationToken);

      if (existingProduct != null)
      {
        return Result<ProductDto>.Failure(
          $"Product with name '{request.ProductName}' already exists",
          ErrorType.Conflict
        );
      }

      // Create product
      var product = _mapper.Map<Domain.Entities.Product>(request);
      product.CreatedAt = DateTime.Now;
      product.UpdatedAt = null;

      await _unitOfWork.Products.AddAsync(product, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);

      var productDto = _mapper.Map<ProductDto>(product);
      return Result<ProductDto>.Success(productDto);
    }
    catch (Exception ex)
    {
      return Result<ProductDto>.Failure(
        $"Failed to create product: {ex.Message}",
        ErrorType.InternalError);
    }
  }
}