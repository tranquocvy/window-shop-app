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
      request.ProductId,
      cancellationToken);

    if (product == null)
    {
      throw new NotFoundException(nameof(Domain.Entities.Product), request.ProductId);
    }

    // Update properties
    product.ProductName = request.ProductName;
    product.BrandName = request.BrandName;
    product.Color = request.Color;
    product.StorageCapacity = request.StorageCapacity;
    product.Processor = request.Processor;
    product.ScreenSize = request.ScreenSize;
    product.BatteryCapacity = request.BatteryCapacity;
    product.ImageUrl = request.ImageUrl;
    product.ImageGalleryJson = request.ImageGalleryJson;
    product.SellPrice = request.SellPrice;
    product.StockQuantity = request.StockQuantity;
    product.Description = request.Description;
    product.IsDraft = request.IsDraft;
    product.UpdatedAt = DateTime.Now;

    await _unitOfWork.Products.UpdateAsync(product);
    await _unitOfWork.SaveChangesAsync();

    return _mapper.Map<ProductDto>(product);
  }
}