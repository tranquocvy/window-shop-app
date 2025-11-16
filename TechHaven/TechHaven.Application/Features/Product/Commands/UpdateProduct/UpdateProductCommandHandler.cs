using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Products;
using AutoMapper;
using TechHaven.Domain.Interfaces;
using TechHaven.Application.Common.Exceptions;

namespace TechHaven.Application.Features.Product.Commands.UpdateProduct;

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, ProductDto>
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

  public async Task<ProductDto> Handle(
    UpdateProductCommand request,
    CancellationToken cancellationToken)
  {
    // Get existing product
    var product = await _unitOfWork.Products.GetByIdAsync(
      request.productDto.ProductId,
      cancellationToken);

    if (product == null)
    {
      throw new NotFoundException(nameof(Domain.Entities.Product), request.productDto.ProductId);
    }

    // Update properties
    product.ProductName = request.productDto.ProductName;
    product.BrandName = request.productDto.BrandName;
    product.Color = request.productDto.Color;
    product.StorageCapacity = request.productDto.StorageCapacity;
    product.Processor = request.productDto.Processor;
    product.ScreenSize = request.productDto.ScreenSize;
    product.BatteryCapacity = request.productDto.BatteryCapacity;
    product.ImageUrl = request.productDto.ImageUrl;
    product.ImageGalleryJson = request.productDto.ImageGalleryJson;
    product.SellPrice = request.productDto.SellPrice;
    product.StockQuantity = request.productDto.StockQuantity;
    product.Description = request.productDto.Description;
    product.IsDraft = request.productDto.IsDraft;
    product.UpdatedAt = DateTime.Now;

    await _unitOfWork.Products.UpdateAsync(product);
    await _unitOfWork.SaveChangesAsync();

    return _mapper.Map<ProductDto>(product);
  }
}