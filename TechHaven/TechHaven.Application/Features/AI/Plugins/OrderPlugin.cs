using System.ComponentModel;
using System.Text;
using Microsoft.SemanticKernel;
using TechHaven.Domain.Enums;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Application.Features.AI.Plugins;

/// <summary>
/// Plugin cung cấp các function để AI truy vấn thông tin đơn hàng
/// </summary>
public class OrderPlugin
{
  private readonly IUnitOfWork _unitOfWork;

  public OrderPlugin(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  private static string GetStatusDisplay(OrderStatus status)
  {
    return status switch
    {
      OrderStatus.Pending => "Chờ xử lý",
      OrderStatus.Processing => "Đang xử lý",
      OrderStatus.Completed => "Hoàn thành",
      OrderStatus.Cancelled => "Đã hủy",
      OrderStatus.Returned => "Đã trả hàng",
      _ => status.ToString()
    };
  }

  [KernelFunction("get_recent_orders")]
  [Description("Lấy danh sách đơn hàng gần đây")]
  public async Task<string> GetRecentOrdersAsync(
      [Description("Số lượng đơn hàng")] int limit = 5,
      CancellationToken cancellationToken = default)
  {
    var orders = await _unitOfWork.Orders.GetRecentOrdersAsync(limit, cancellationToken);

    if (!orders.Any())
      return "Chưa có đơn hàng nào";

    var sb = new StringBuilder("Đơn hàng gần đây:\n\n");
    foreach (var order in orders)
    {
      sb.AppendLine($"Đơn #{order.OrderId}");
      sb.AppendLine($"Khách hàng: {order.Customer?.CustomerName ?? "Khách vãng lai"}");
      sb.AppendLine($"Ngày: {order.OrderDate:dd/MM/yyyy HH:mm}");
      sb.AppendLine($"Tổng tiền: {order.TotalAmount:N0} VND");
      sb.AppendLine($"Trạng thái: {GetStatusDisplay(order.Status)}\n");
    }

    return sb.ToString();
  }

  [KernelFunction("get_order_details")]
  [Description("Lấy thông tin chi tiết của một đơn hàng")]
  public async Task<string> GetOrderDetailsAsync(
      [Description("ID của đơn hàng")] int orderId,
      CancellationToken cancellationToken = default)
  {
    var order = await _unitOfWork.Orders.GetWithDetailsAsync(orderId, cancellationToken);

    if (order == null)
      return $"Không tìm thấy đơn hàng #{orderId}";

    var sb = new StringBuilder();
    sb.AppendLine($"Chi tiết đơn hàng #{order.OrderId}:");
    sb.AppendLine();
    sb.AppendLine($"Khách hàng: {order.Customer?.CustomerName ?? "Khách vãng lai"}");
    sb.AppendLine($"Ngày đặt: {order.OrderDate:dd/MM/yyyy HH:mm}");
    sb.AppendLine($"Nhân viên: {order.User?.UserFullName ?? "(Không rõ)"}");
    sb.AppendLine($"Trạng thái: {GetStatusDisplay(order.Status)}");
    sb.AppendLine();
    sb.AppendLine("Sản phẩm:");
    if (order.OrderDetails != null && order.OrderDetails.Any())
    {
      foreach (var item in order.OrderDetails)
      {
        sb.AppendLine($"- {item.Product?.ProductName ?? "(Không rõ)"} x{item.Quantity}");
        sb.AppendLine($"  Đơn giá: {item.UnitPrice:N0} VND");
        sb.AppendLine($"  Thành tiền: {item.SubTotal:N0} VND");
      }
    }
    else
    {
      sb.AppendLine("- (Không có sản phẩm)");
    }

    sb.AppendLine();
    sb.AppendLine($"Tổng cộng: {order.SubtotalAmount:N0} VND");
    if (order.Discount > 0)
      sb.AppendLine($"Giảm giá: {order.Discount:N0} VND");
    sb.AppendLine($"Tổng thanh toán: {order.TotalAmount:N0} VND");

    return sb.ToString();
  }

  [KernelFunction("get_today_revenue")]
  [Description("Lấy thống kê doanh thu hôm nay")]
  public async Task<string> GetTodayRevenueAsync(
      CancellationToken cancellationToken = default)
  {
    var revenue = await _unitOfWork.Orders.GetTodayRevenueAsync(cancellationToken);
    var orderCount = await _unitOfWork.Orders.GetTodayOrderCountAsync(cancellationToken);

    return $"Thống kê hôm nay:\n" +
           $"- Số đơn hàng: {orderCount}\n" +
           $"- Doanh thu: {revenue:N0} VND\n" +
           $"- Trung bình/đơn: {(orderCount > 0 ? revenue / orderCount : 0):N0} VND";
  }
}