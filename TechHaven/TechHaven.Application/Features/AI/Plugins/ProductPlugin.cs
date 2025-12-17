using System.ComponentModel;
using Microsoft.SemanticKernel;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.SearchCriteria;

namespace TechHaven.Application.Features.AI.Plugins;

/// <summary>
/// Plugin cung cấp các function để AI truy vấn thông tin sản phẩm
/// </summary>
public class ProductPlugin
{
  private readonly IUnitOfWork _unitOfWork;

  public ProductPlugin(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  [KernelFunction("search_products")]
  [Description("Tìm kiếm sản phẩm theo tên, hãng, hoặc từ khóa")]
  public async Task<string> SearchProductsAsync(
      [Description("Từ khóa tìm kiếm (tên sản phẩm, hãng)")] string searchTerm,
      [Description("Số lượng kết quả tối đa")] int limit = 5,
      CancellationToken cancellationToken = default)
  {
    var criteria = new ProductSearchCriteria
    {
      SearchTerm = searchTerm,
      PageNumber = 1,
      PageSize = limit,
      IsDraft = false
    };

    var (products, _) = await _unitOfWork.Products
        .SearchWithPaginationAsync(criteria, cancellationToken);

    if (!products.Any())
      return $"Không tìm thấy sản phẩm nào với từ khóa '{searchTerm}'";

    var result = "Danh sách sản phẩm:\n";
    foreach (var p in products)
    {
      result += $"- {p.ProductName} ({p.BrandName})\n";
      result += $"  Giá: {p.SellPrice:N0} VND\n";
      result += $"  Tồn kho: {p.StockQuantity} sản phẩm\n";
      result += $"  ID: {p.ProductId}\n\n";
    }

    return result;
  }

  [KernelFunction("get_product_details")]
  [Description("Lấy thông tin chi tiết về một sản phẩm cụ thể")]
  public async Task<string> GetProductDetailsAsync(
      [Description("ID của sản phẩm")] int productId,
      CancellationToken cancellationToken = default)
  {
    var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);

    if (product == null)
      return $"Không tìm thấy sản phẩm với ID {productId}";

    var details = $"Thông tin chi tiết sản phẩm:\n\n";
    details += $"Tên: {product.ProductName}\n";
    details += $"Hãng: {product.BrandName}\n";
    details += $"Giá: {product.SellPrice:N0} VND\n";
    details += $"Giá vốn: {product.CostPrice:N0} VND\n";
    details += $"Tồn kho: {product.StockQuantity} sản phẩm\n";

    if (!string.IsNullOrEmpty(product.Color))
      details += $"Màu sắc: {product.Color}\n";

    if (product.StorageCapacity.HasValue)
      details += $"Bộ nhớ: {product.StorageCapacity} GB\n";

    if (!string.IsNullOrEmpty(product.Processor))
      details += $"CPU: {product.Processor}\n";

    if (product.ScreenSize.HasValue)
      details += $"Màn hình: {product.ScreenSize} inch\n";

    if (!string.IsNullOrEmpty(product.Description))
      details += $"\nMô tả: {product.Description}\n";

    return details;
  }

  [KernelFunction("check_stock")]
  [Description("Kiểm tra tồn kho của một sản phẩm")]
  public async Task<string> CheckStockAsync(
      [Description("ID của sản phẩm")] int productId,
      CancellationToken cancellationToken = default)
  {
    var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);

    if (product == null)
      return $"Không tìm thấy sản phẩm với ID {productId}";

    var status = product.StockQuantity switch
    {
      0 => "HẾT HÀNG",
      <= 5 => $"SẮP HẾT (còn {product.StockQuantity} sản phẩm)",
      _ => $"CÒN HÀNG ({product.StockQuantity} sản phẩm)"
    };

    return $"{product.ProductName}: {status}";
  }

  [KernelFunction("get_products_by_brand")]
  [Description("Lấy danh sách sản phẩm theo hãng")]
  public async Task<string> GetProductsByBrandAsync(
      [Description("Tên hãng (Apple, Samsung, Xiaomi...)")] string brandName,
      [Description("Số lượng kết quả")] int limit = 5,
      CancellationToken cancellationToken = default)
  {
    var criteria = new ProductSearchCriteria
    {
      Brand = brandName,
      PageNumber = 1,
      PageSize = limit,
      IsDraft = false
    };

    var (products, totalCount) = await _unitOfWork.Products
        .SearchWithPaginationAsync(criteria, cancellationToken);

    if (!products.Any())
      return $"Không tìm thấy sản phẩm nào của hãng {brandName}";

    var result = $"Sản phẩm {brandName} (Tổng: {totalCount}):\n\n";
    foreach (var p in products)
    {
      result += $"- {p.ProductName}\n";
      result += $"  Giá: {p.SellPrice:N0} VND\n";
      result += $"  Tồn kho: {p.StockQuantity}\n\n";
    }

    return result;
  }
}