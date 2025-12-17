using System.ComponentModel;
using Microsoft.SemanticKernel;
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

  [KernelFunction("get_recent_orders")]
  [Description("Lấy danh sách đơn hàng gần đây")]
  public async Task<string> GetRecentOrdersAsync(
      [Description("Số lượng đơn hàng")] int limit = 5,
      CancellationToken cancellationToken = default)
  {
    var orders = await _unitOfWork.Orders
        .GetRecentOrdersAsync(limit, cancellationToken);

    if (!orders.Any())
      return "Chưa có đơn hàng nào";

    var result = "Đơn hàng gần đây:\n\n";
    foreach (var order in orders)
    {
      result += $"Đơn #{order.OrderId}\n";
      result += $"Khách hàng: {order.Customer?.CustomerName ?? "Khách vãng lai"}\n";
      result += $"Ngày: {order.OrderDate:dd/MM/yyyy HH:mm}\n";
      result += $"Tổng tiền: {order.TotalAmount:N0} VND\n";
      result += $"Trạng thái: {order.Status}\n\n";
    }

    return result;
  }

  [KernelFunction("get_order_details")]
  [Description("Lấy thông tin chi tiết của một đơn hàng")]
  public async Task<string> GetOrderDetailsAsync(
      [Description("ID của đơn hàng")] int orderId,
      CancellationToken cancellationToken = default)
  {
    var order = await _unitOfWork.Orders
        .GetWithDetailsAsync(orderId, cancellationToken);

    if (order == null)
      return $"Không tìm thấy đơn hàng #{orderId}";

    var details = $"Chi tiết đơn hàng #{order.OrderId}:\n\n";
    details += $"Khách hàng: {order.Customer?.CustomerName ?? "Khách vãng lai"}\n";
    details += $"Ngày đặt: {order.OrderDate:dd/MM/yyyy HH:mm}\n";
    details += $"Nhân viên: {order.User?.UserFullName}\n";
    details += $"Trạng thái: {order.Status}\n\n";

    details += "Sản phẩm:\n";
    if (order.OrderDetails != null)
    {
      foreach (var item in order.OrderDetails)
      {
        details += $"- {item.Product?.ProductName} x{item.Quantity}\n";
        details += $"  Đơn giá: {item.UnitPrice:N0} VND\n";
        details += $"  Thành tiền: {item.SubTotal:N0} VND\n";
      }
    }

    details += $"\nTổng cộng: {order.SubtotalAmount:N0} VND\n";
    if (order.Discount > 0)
      details += $"Giảm giá: {order.Discount:N0} VND\n";
    details += $"Tổng thanh toán: {order.TotalAmount:N0} VND\n";

    return details;
  }

  [KernelFunction("get_today_revenue")]
  [Description("Lấy thống kê doanh thu hôm nay")]
  public async Task<string> GetTodayRevenueAsync(
      CancellationToken cancellationToken = default)
  {
    var revenue = await _unitOfWork.Orders
        .GetTodayRevenueAsync(cancellationToken);

    var orderCount = await _unitOfWork.Orders
        .GetTodayOrderCountAsync(cancellationToken);

    return $"Thống kê hôm nay:\n" +
           $"- Số đơn hàng: {orderCount}\n" +
           $"- Doanh thu: {revenue:N0} VND\n" +
           $"- Trung bình/đơn: {(orderCount > 0 ? revenue / orderCount : 0):N0} VND";
  }
}