using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Products;
using AutoMapper;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Application.Common.Exceptions;

namespace TechHaven.Application.Features.Product.Commands.UpdateProduct;

public class UpdateProductCommandHandler
  : ICommandHandler<UpdateProductCommand, Result<ProductDto>>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public UpdateProductCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper)
  {
    _unitOfWork = unitOfWork;
    _mapper = mapper;
  }

  public async Task<Result<ProductDto>> Handle(
    UpdateProductCommand request,
    CancellationToken cancellationToken)
  {
    try
    {
      // Get existing product
      var product = await _unitOfWork.Products.GetByIdAsync(
        request.ProductId,
        cancellationToken);

      if (product == null)
      {
        return Result<ProductDto>.Failure(
          $"Product {request.ProductName} not found.",
          ErrorType.NotFound
        );
      }

      _mapper.Map(request, product);
      product.UpdatedAt = DateTime.UtcNow;

      await _unitOfWork.Products.UpdateAsync(product);
      await _unitOfWork.SaveChangesAsync();

      var productDto = _mapper.Map<ProductDto>(product);

      return Result<ProductDto>.Success(productDto);
    }
    catch (Exception ex)
    {
      return Result<ProductDto>.Failure(
        $"Failed to update product: {ex.Message}",
        ErrorType.InternalError);
    }
  }
}