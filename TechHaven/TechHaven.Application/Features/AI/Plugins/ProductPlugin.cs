using System.ComponentModel;
using System.Text;
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

    var sb = new StringBuilder();
    sb.AppendLine("Danh sách sản phẩm:");
    foreach (var p in products)
    {
      sb.AppendLine($"- {p.ProductName} ({p.BrandName})");
      sb.AppendLine($"  Giá: {p.SellPrice:N0} VND");
      sb.AppendLine($"  Tồn kho: {p.StockQuantity} sản phẩm {GetStockStatus(p.StockQuantity)}");
      sb.AppendLine($"  ID: {p.ProductId}");
      sb.AppendLine();
    }

    return sb.ToString();
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

    var sb = new StringBuilder();
    sb.AppendLine("Thông tin chi tiết sản phẩm:\n");
    sb.AppendLine($"Tên: {product.ProductName}");
    sb.AppendLine($"Hãng: {product.BrandName}");
    sb.AppendLine($"Giá bán: {product.SellPrice:N0} VND");
    sb.AppendLine($"Giá vốn: {product.CostPrice:N0} VND");
    sb.AppendLine($"Tồn kho: {product.StockQuantity} sản phẩm {GetStockStatus(product.StockQuantity)}");

    if (!string.IsNullOrWhiteSpace(product.Color))
      sb.AppendLine($"Màu sắc: {product.Color}");

    if (product.StorageCapacity.HasValue)
      sb.AppendLine($"Bộ nhớ: {product.StorageCapacity} GB");

    if (!string.IsNullOrWhiteSpace(product.Processor))
      sb.AppendLine($"CPU: {product.Processor}");

    if (product.ScreenSize.HasValue)
      sb.AppendLine($"Màn hình: {product.ScreenSize} inch");

    if (product.BatteryCapacity.HasValue)
      sb.AppendLine($"Pin: {product.BatteryCapacity} mAh");

    if (!string.IsNullOrWhiteSpace(product.Description))
      sb.AppendLine($"\nMô tả: {product.Description}");

    return sb.ToString();
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

    var status = GetStockStatus(product.StockQuantity, true);

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

    var sb = new StringBuilder();
    sb.AppendLine($"Sản phẩm {brandName} (Tổng: {totalCount}):\n");
    foreach (var p in products)
    {
      sb.AppendLine($"- {p.ProductName}");
      sb.AppendLine($"  Giá: {p.SellPrice:N0} VND");
      sb.AppendLine($"  Tồn kho: {p.StockQuantity} sản phẩm {GetStockStatus(p.StockQuantity)}");
      sb.AppendLine();
    }

    return sb.ToString();
  }

  private static string GetStockStatus(int stock, bool shortForm = false)
  {
    if (stock == 0)
      return shortForm ? "HẾT HÀNG" : "(Hết hàng)";
    if (stock <= 5)
      return shortForm ? $"SẮP HẾT (còn {stock})" : "(Sắp hết)";
    return shortForm ? $"CÒN HÀNG ({stock})" : "(Còn hàng)";
  }
}