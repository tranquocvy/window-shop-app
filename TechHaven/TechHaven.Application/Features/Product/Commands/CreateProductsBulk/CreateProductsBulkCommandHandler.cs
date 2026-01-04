// TechHaven.Application/Features/Product/Commands/CreateProductsBulk/CreateProductsBulkCommandHandler.cs
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using TechHaven.Application.Features.Product.Commands.CreateProduct;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Application.Features.Product.Commands.CreateProductsBulk;

public class CreateProductsBulkCommandHandler
    : ICommandHandler<CreateProductsBulkCommand, Result<ProductBulkCreateResponseDto>>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;
  private readonly ILogger<CreateProductsBulkCommandHandler> _logger;
  private readonly IValidator<CreateProductCommand> _productValidator;

  public CreateProductsBulkCommandHandler(
      IUnitOfWork unitOfWork,
      IMapper mapper,
      ILogger<CreateProductsBulkCommandHandler> logger,
      IValidator<CreateProductCommand> productValidator)
  {
    _unitOfWork = unitOfWork;
    _mapper = mapper;
    _logger = logger;
    _productValidator = productValidator;
  }

  public async Task<Result<ProductBulkCreateResponseDto>> Handle(
      CreateProductsBulkCommand request,
      CancellationToken cancellationToken)
  {
    var response = new ProductBulkCreateResponseDto
    {
      TotalProducts = request.Products.Count
    };

    if (request.Products.Count == 0)
    {
      return Result<ProductBulkCreateResponseDto>.Failure(
          "No products provided for import",
          ErrorType.Validation);
    }

    try
    {
      _logger.LogInformation(
          "Starting bulk product import: {Count} products, SkipDuplicates: {Skip}, ValidateFirst: {Validate}",
          request.Products.Count, request.SkipDuplicates, request.ValidateBeforeInsert);

      // Step 1: Get existing product names for duplicate check
      var existingProductNames = new HashSet<string>(
          (await _unitOfWork.Products.GetAllAsync(cancellationToken))
              .Select(p => p.ProductName.ToLower()),
          StringComparer.OrdinalIgnoreCase);

      // Step 2: Validate all products first (if enabled)
      var validationErrors = new List<ProductImportError>();
      var productsToCreate = new List<(int RowIndex, ProductUpsertRequest Request, CreateProductCommand Command)>();

      for (int i = 0; i < request.Products.Count; i++)
      {
        var productRequest = request.Products[i];
        var rowIndex = i + 2; // Excel row (1 = header, data starts at 2)

        // Check duplicate
        if (existingProductNames.Contains(productRequest.ProductName.ToLower()))
        {
          if (request.SkipDuplicates)
          {
            response.SkippedCount++;
            _logger.LogDebug(
                "Row {Row}: Skipping duplicate product '{Name}'",
                rowIndex, productRequest.ProductName);
            continue;
          }

          validationErrors.Add(new ProductImportError
          {
            RowIndex = rowIndex,
            ProductName = productRequest.ProductName,
            ErrorMessages = new List<string> { "Product with this name already exists" }
          });
          continue;
        }

        // Map to command for validation
        var command = new CreateProductCommand
        {
          ProductName = productRequest.ProductName,
          BrandName = productRequest.BrandName,
          Color = productRequest.Color,
          StorageCapacity = productRequest.StorageCapacity,
          Processor = productRequest.Processor,
          ScreenSize = productRequest.ScreenSize,
          BatteryCapacity = productRequest.BatteryCapacity,
          ImageUrl = productRequest.ImageUrl,
          ImageGalleryJson = productRequest.ImageGalleryJson,
          CostPrice = productRequest.CostPrice,
          SellPrice = productRequest.SellPrice,
          StockQuantity = productRequest.StockQuantity,
          Description = productRequest.Description,
          IsDraft = productRequest.IsDraft
        };

        // Validate using FluentValidation
        var validationResult = await _productValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
          validationErrors.Add(new ProductImportError
          {
            RowIndex = rowIndex,
            ProductName = productRequest.ProductName,
            ErrorMessages = validationResult.Errors
                  .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                  .ToList()
          });
          continue;
        }

        productsToCreate.Add((rowIndex, productRequest, command));
      }

      if (request.ValidateBeforeInsert && validationErrors.Count > 0)
      {
        response.FailedCount = validationErrors.Count;
        response.Errors = validationErrors;

        _logger.LogWarning(
            "Bulk import failed validation: {ErrorCount} errors found",
            validationErrors.Count);

        // Trả về Success với response chứa thông tin lỗi validation
        return Result<ProductBulkCreateResponseDto>.Success(response);
      }

      // Step 3: Insert products in a transaction
      // Removed manual transaction - let EF Core handle it with retry strategy
      
      try
      {
        var createdProducts = new List<Domain.Entities.Product>();

        foreach (var (rowIndex, productRequest, command) in productsToCreate)
        {
          try
          {
            var product = _mapper.Map<Domain.Entities.Product>(command);
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = null;

            await _unitOfWork.Products.AddAsync(product, cancellationToken);
            createdProducts.Add(product);

            // Chuyển log xuống sau khi SaveChanges thành công
          }
          catch (Exception ex)
          {
            _logger.LogError(ex,
                "Row {Row}: Failed to create product '{Name}'",
                rowIndex, productRequest.ProductName);

            validationErrors.Add(new ProductImportError
            {
              RowIndex = rowIndex,
              ProductName = productRequest.ProductName,
              ErrorMessages = new List<string> { ex.Message }
            });

            if (request.ValidateBeforeInsert)
            {
              throw; // Rollback if atomic mode
            }
          }
        }

        // Save all changes at once
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log success chỉ sau khi save thành công
        foreach (var product in createdProducts)
        {
          _logger.LogDebug(
              "Successfully created product '{Name}'",
              product.ProductName);
        }

        // Map created products to DTOs
        response.CreatedProducts = _mapper.Map<List<ProductDto>>(createdProducts);
        response.SuccessCount = createdProducts.Count;
        response.FailedCount = validationErrors.Count;
        response.Errors = validationErrors;

        _logger.LogInformation(
            "Bulk import completed: {Success} created, {Failed} failed, {Skipped} skipped",
            response.SuccessCount, response.FailedCount, response.SkippedCount);

        return Result<ProductBulkCreateResponseDto>.Success(response);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to save products to database");
        throw;
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Bulk product import failed");

      return Result<ProductBulkCreateResponseDto>.Failure(
          $"Bulk import failed: {ex.Message}",
          ErrorType.InternalError);
    }
  }
}