using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Entities;
using TechHaven.Infrastructure.Persistence;
using TechHaven.Presentation.WebAPI.Controllers;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Presentation.WebAPI.Seeders;

/// <summary>
/// Reads scraped cellphone data, uploads media to Supabase via ImageController,
/// and persists <see cref="Product" /> entities.
/// </summary>
public class CellphoneProductSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<CellphoneProductSeeder> _logger;
    private readonly ImageController _imageController;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CellphoneProductSeeder(
        AppDbContext context,
        IImageUploadService imageUploadService,
        ILogger<CellphoneProductSeeder> logger,
        ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = logger;
        _imageController = new ImageController(
            imageUploadService,
            loggerFactory.CreateLogger<ImageController>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    public async Task SeedAsync(
        bool overwriteExisting = false,
        CancellationToken cancellationToken = default)
    {
        if (!overwriteExisting && await _context.Products.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Products already exist. Skipping cellphone seed import.");
            return;
        }

        var seedRoot = ResolveSeedRoot();
        if (!Directory.Exists(seedRoot))
        {
            _logger.LogWarning(
                "Cellphone seed folder not found at {Path}. Nothing to import.",
                seedRoot);
            return;
        }

        var productsToInsert = new List<Product>();

        foreach (var brandFolder in Directory.EnumerateDirectories(seedRoot))
        {
            foreach (var productFolder in Directory.EnumerateDirectories(brandFolder))
            {
                var jsonPath = Path.Combine(productFolder, "data.json");
                if (!File.Exists(jsonPath))
                {
                    continue;
                }

                var payload = await DeserializePayloadAsync(jsonPath, cancellationToken);
                if (payload is null || string.IsNullOrWhiteSpace(payload.ProductName))
                {
                    _logger.LogWarning("Invalid payload detected in {File}", jsonPath);
                    continue;
                }

                if (!overwriteExisting &&
                    await _context.Products.AnyAsync(
                        p => p.ProductName == payload.ProductName,
                        cancellationToken))
                {
                    _logger.LogDebug("Product {Name} already exists. Skipping.", payload.ProductName);
                    continue;
                }

                var folderSlug = BuildFolderSlug(payload.BrandName);
                var mainImageUrl = await UploadImageAsync(
                    productFolder,
                    payload.ImageUrl,
                    folderSlug,
                    cancellationToken);

                var galleryUrls = await UploadGalleryAsync(
                    productFolder,
                    payload.ImageGalleryJson,
                    folderSlug,
                    cancellationToken);

                var product = new Product
                {
                    ProductName = payload.ProductName,
                    BrandName = payload.BrandName,
                    Color = payload.Color,
                    StorageCapacity = payload.StorageCapacity,
                    Processor = payload.Processor,
                    ScreenSize = payload.ScreenSize.HasValue
                        ? (decimal?)Math.Round(payload.ScreenSize.Value, 2)
                        : null,
                    BatteryCapacity = payload.BatteryCapacity,
                    ImageUrl = mainImageUrl,
                    ImageGalleryJson = JsonSerializer.Serialize(galleryUrls),
                    CostPrice = CalculateCostPrice(payload.SellPrice),
                    SellPrice = payload.SellPrice,
                    StockQuantity = 20,
                    Description = payload.Description,
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                };

                productsToInsert.Add(product);
            }
        }

        if (productsToInsert.Count == 0)
        {
            _logger.LogWarning("No cellphone products were queued for insertion.");
            return;
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            if (overwriteExisting)
            {
                _context.Products.RemoveRange(_context.Products);
            }

            await _context.Products.AddRangeAsync(productsToInsert, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });

        _logger.LogInformation("Seeded {Count} cellphone products.", productsToInsert.Count);
    }

    private static string ResolveSeedRoot()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var solutionRoot = Path.GetFullPath(Path.Combine(projectRoot, ".."));
        return Path.Combine(
            solutionRoot,
            "TechHaven.Infrastructure",
            "Data",
            "ProductSeeder",
            "cellphones",
            "downloaded_data");
    }

    private static decimal CalculateCostPrice(decimal sellPrice)
    {
        if (sellPrice <= 0)
        {
            return 0;
        }

        var cost = sellPrice * 0.85m;
        return Math.Round(cost, 2, MidpointRounding.AwayFromZero);
    }

    private static string BuildFolderSlug(string brandName)
    {
        var slug = brandName?.Trim().ToLowerInvariant() ?? "unknown";
        return $"products/{slug}";
    }

    private static async Task<ProductSeedPayload?> DeserializePayloadAsync(
        string jsonPath,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(jsonPath);
        return await JsonSerializer.DeserializeAsync<ProductSeedPayload>(stream, JsonOptions, cancellationToken);
    }

    private async Task<string?> UploadImageAsync(
        string productFolder,
        string? relativeFileName,
        string folderSlug,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(relativeFileName))
        {
            return null;
        }

        var absolutePath = Path.Combine(productFolder, relativeFileName);
        if (!File.Exists(absolutePath))
        {
            _logger.LogWarning("Image file not found: {Path}", absolutePath);
            return null;
        }

        await using var stream = File.OpenRead(absolutePath);
        var formFile = new FormFile(stream, 0, stream.Length, "file", Path.GetFileName(absolutePath))
        {
            Headers = new HeaderDictionary(),
            ContentType = GetContentType(absolutePath)
        };

        var result = await _imageController.UploadImage(formFile, folderSlug, cancellationToken);
        if (result is OkObjectResult okResult &&
            okResult.Value is ResponseWrapper<ImageUploadResponse> wrapper &&
            wrapper.Success &&
            wrapper.Data is not null)
        {
            return wrapper.Data.ImageUrl;
        }

        LogUploadFailure(result, absolutePath);
        return null;
    }

    private async Task<List<string>> UploadGalleryAsync(
        string productFolder,
        string? galleryJson,
        string folderSlug,
        CancellationToken cancellationToken)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(galleryJson))
        {
            return result;
        }

        List<string>? galleryFiles;
        try
        {
            galleryFiles = JsonSerializer.Deserialize<List<string>>(galleryJson, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse gallery JSON: {Json}", galleryJson);
            return result;
        }

        if (galleryFiles is null)
        {
            return result;
        }

        foreach (var fileName in galleryFiles)
        {
            var url = await UploadImageAsync(productFolder, fileName, folderSlug, cancellationToken);
            if (!string.IsNullOrEmpty(url))
            {
                result.Add(url);
            }
        }

        return result;
    }

    private void LogUploadFailure(IActionResult result, string filePath)
    {
        if (result is ObjectResult objectResult)
        {
            _logger.LogWarning(
                "Uploading {File} failed with status {Status} and payload {Payload}",
                filePath,
                objectResult.StatusCode,
                JsonSerializer.Serialize(objectResult.Value));
        }
        else
        {
            _logger.LogWarning("Uploading {File} failed with unknown error.", filePath);
        }
    }

    private static string GetContentType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
    }

    private sealed class ProductSeedPayload
    {
        public string ProductName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string? Color { get; set; }
        public int? StorageCapacity { get; set; }
        public string? Processor { get; set; }
        public double? ScreenSize { get; set; }
        public int? BatteryCapacity { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageGalleryJson { get; set; }
        public decimal SellPrice { get; set; }
        public string? Description { get; set; }
    }
}

