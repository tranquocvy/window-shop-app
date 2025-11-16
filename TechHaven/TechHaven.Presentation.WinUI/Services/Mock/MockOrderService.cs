using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Orders;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using Shared.DTOs.Common;

namespace TechHaven.Presentation.WinUI.Services.Mock
{
    public class MockOrderService : IOrderService
    {
        private readonly List<OrderDto> _mockOrders;

        public MockOrderService()
        {
            _mockOrders = new List<OrderDto>
            {
                new OrderDto
                {
                    OrderId = 1,
                    CustomerId = 1001,
                    CustomerName = "Nguyễn Văn A",
                    UserId = 1,
                    UserFullName = "Admin",
                    OrderDate = DateTime.Now.AddDays(-3),
                    Status = OrderStatus.Completed,
                    SubtotalAmount = 28990000m,
                    Discount = 0m,
                    TotalAmount = 28990000m,
                    Notes = "Giao hàng nhanh",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 1, ProductName = "iPhone 15 Pro", UnitPrice = 28990000m, Quantity = 1, SubTotal = 28990000m }
                    }
                },
                new OrderDto
                {
                    OrderId = 2,
                    CustomerId = 1002,
                    CustomerName = "Trần Thị B",
                    UserId = 2,
                    UserFullName = "Sales01",
                    OrderDate = DateTime.Now.AddDays(-1),
                    Status = OrderStatus.Processing,
                    SubtotalAmount = 25990000m,
                    Discount = 1000000m,
                    TotalAmount = 24990000m,
                    Notes = "Khách hẹn lấy tại cửa hàng",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 2, ProductName = "Samsung Galaxy S24 Ultra", UnitPrice = 25990000m, Quantity = 1, SubTotal = 25990000m }
                    }
                },
                new OrderDto
                {
                    OrderId = 3,
                    CustomerId = null,
                    CustomerName = "Khách lẻ",
                    UserId = 1,
                    UserFullName = "Admin",
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending,
                    SubtotalAmount = 14990000m,
                    Discount = 0m,
                    TotalAmount = 14990000m,
                    Notes = null,
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 3, ProductName = "Xiaomi 14T", UnitPrice = 14990000m, Quantity = 1, SubTotal = 14990000m }
                    }
                }
            };
        }

        public Task<ResponseWrapper<List<OrderDto>>> GetAllOrdersAsync()
        {
            var response = new ResponseWrapper<List<OrderDto>>
            {
                Success = true,
                Message = "Orders retrieved successfully",
                Data = _mockOrders
            };
            return Task.FromResult(response);
        }

        public Task<ResponseWrapper<OrderDto>> GetOrderByIdAsync(int id)
        {
            var order = _mockOrders.FirstOrDefault(o => o.OrderId == id);
            var response = new ResponseWrapper<OrderDto>
            {
                Success = order != null,
                Message = order != null ? "Order retrieved successfully" : "Order not found",
                Data = order
            };
            return Task.FromResult(response);
        }

        public Task<ResponseWrapper<OrderDto>> CreateOrderAsync(OrderCreateDto dto)
        {
            var newId = _mockOrders.Any() ? _mockOrders.Max(o => o.OrderId) + 1 : 1;

            var details = (dto.Items ?? Array.Empty<OrderCreateItemDto>())
                .Select(i => new OrderDetailDto
                {
                    ProductId = i.ProductId,
                    ProductName = string.Empty,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    SubTotal = i.UnitPrice * i.Quantity
                })
                .ToList();

            var subtotal = details.Sum(d => d.SubTotal);
            var total = subtotal - dto.Discount;

            var order = new OrderDto
            {
                OrderId = newId,
                CustomerId = dto.CustomerId,
                CustomerName = null,
                UserId = 1,
                UserFullName = "Admin",
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                SubtotalAmount = subtotal,
                Discount = dto.Discount,
                TotalAmount = total,
                Notes = dto.Notes,
                Details = details
            };

            _mockOrders.Add(order);
            
            var response = new ResponseWrapper<OrderDto>
            {
                Success = true,
                Message = "Order created successfully",
                Data = order
            };
            return Task.FromResult(response);
        }

        public Task<ResponseWrapper<bool>> DeleteOrderAsync(int id)
        {
            var existing = _mockOrders.FirstOrDefault(o => o.OrderId == id);
            bool success = false;
            
            if (existing != null)
            {
                _mockOrders.Remove(existing);
                success = true;
            }
            
            var response = new ResponseWrapper<bool>
            {
                Success = success,
                Message = success ? "Order deleted successfully" : "Order not found",
                Data = success
            };
            return Task.FromResult(response);
        }

        public Task<ResponseWrapper<OrderDto>> UpdateOrderStatusAsync(OrderUpdateStatusDto dto)
        {
            var existing = _mockOrders.FirstOrDefault(o => o.OrderId == dto.OrderId);
            
            if (existing != null)
            {
                existing.Status = dto.Status;
            }
            
            var response = new ResponseWrapper<OrderDto>
            {
                Success = existing != null,
                Message = existing != null ? "Order status updated successfully" : "Order not found",
                Data = existing
            };
            return Task.FromResult(response);
        }

        public Task<ResponseWrapper<List<OrderDto>>> QueryOrdersAsync(OrderQueryDto query)
        {
            IEnumerable<OrderDto> result = _mockOrders;

            // Lọc theo trạng thái đơn hàng
            if (query.Status.HasValue)
                result = result.Where(o => o.Status == query.Status.Value);

            // Lọc theo từ khóa khách hàng
            if (!string.IsNullOrWhiteSpace(query.CustomerKeyword))
            {
                var keyword = query.CustomerKeyword.Trim();
                result = result.Where(o =>
                    (o.CustomerName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (o.Notes?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            // Lọc theo ngày (OrderDate là DateRangeFilter)
            if (query.OrderDate != null)
            {
                if (query.OrderDate.StartDate.HasValue)
                    result = result.Where(o => o.OrderDate >= query.OrderDate.StartDate.Value);

                if (query.OrderDate.EndDate.HasValue)
                    result = result.Where(o => o.OrderDate <= query.OrderDate.EndDate.Value);
            }

            // Sắp xếp
            if (query.Sorting != null)
            {
                if (query.Sorting.SortBy?.Equals("date", StringComparison.OrdinalIgnoreCase) == true)
                {
                    result = query.Sorting.Desc
                        ? result.OrderByDescending(o => o.OrderDate)
                        : result.OrderBy(o => o.OrderDate);
                }
                else if (query.Sorting.SortBy?.Equals("total", StringComparison.OrdinalIgnoreCase) == true)
                {
                    result = query.Sorting.Desc
                        ? result.OrderByDescending(o => o.TotalAmount)
                        : result.OrderBy(o => o.TotalAmount);
                }
                else if (query.Sorting.SortBy?.Equals("customer", StringComparison.OrdinalIgnoreCase) == true)
                {
                    result = query.Sorting.Desc
                        ? result.OrderByDescending(o => o.CustomerName)
                        : result.OrderBy(o => o.CustomerName);
                }
            }

            // Phân trang
            if (query.PageNumber > 0 && query.PageSize > 0)
            {
                result = result
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize);
            }

            var response = new ResponseWrapper<List<OrderDto>>
            {
                Success = true,
                Message = "Orders queried successfully",
                Data = result.ToList()
            };
            return Task.FromResult(response);
        }
    }
}
