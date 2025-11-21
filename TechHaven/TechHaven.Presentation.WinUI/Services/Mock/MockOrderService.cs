using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Orders;

namespace TechHaven.Presentation.WinUI.Services.Mock
{
    public class MockOrderService : IOrderService
    {
        private readonly List<OrderDto> _mockOrders;
        private int _nextOrderId;

        public MockOrderService()
        {
            _nextOrderId = 16;
            _mockOrders = new List<OrderDto>
            {
                // Orders từ 7 ngày trước
                new OrderDto
                {
                    OrderId = 1,
                    CustomerId = 1,
                    CustomerName = "Nguyễn Văn A",
                    UserId = 1,
                    UserFullName = "Nguyễn Khắc Vượng",
                    OrderDate = DateTime.Now.AddDays(-7),
                    Status = OrderStatus.Completed,
                    SubtotalAmount = 28990000m,
                    Discount = 1000000m,
                    TotalAmount = 27990000m,
                    Notes = "Giao hàng nhanh, khách hàng VIP",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 1, ProductName = "iPhone 15 Pro", UnitPrice = 28990000m, Quantity = 1, SubTotal = 28990000m }
                    }
                },
                new OrderDto
                {
                    OrderId = 2,
                    CustomerId = 2,
                    CustomerName = "Trần Thị B",
                    UserId = 2,
                    UserFullName = "Nguyễn Phúc Hậu",
                    OrderDate = DateTime.Now.AddDays(-6),
                    Status = OrderStatus.Completed,
                    SubtotalAmount = 51980000m,
                    Discount = 2000000m,
                    TotalAmount = 49980000m,
                    Notes = "Mua 2 máy, giảm giá combo",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 11, ProductName = "Samsung Galaxy S24 Ultra", UnitPrice = 25990000m, Quantity = 2, SubTotal = 51980000m }
                    }
                },
                
                // Orders từ 5 ngày trước
                new OrderDto
                {
                    OrderId = 3,
                    CustomerId = 3,
                    CustomerName = "Phạm Minh C",
                    UserId = 1,
                    UserFullName = "Nguyễn Khắc Vượng",
                    OrderDate = DateTime.Now.AddDays(-5),
                    Status = OrderStatus.Completed,
                    SubtotalAmount = 14990000m,
                    Discount = 0m,
                    TotalAmount = 14990000m,
                    Notes = "Khách VIP, được tư vấn kỹ",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 21, ProductName = "Xiaomi 14T", UnitPrice = 14990000m, Quantity = 1, SubTotal = 14990000m }
                    }
                },
                new OrderDto
                {
                    OrderId = 4,
                    CustomerId = null,
                    CustomerName = "Khách lẻ",
                    UserId = 2,
                    UserFullName = "Nguyễn Phúc Hậu",
                    OrderDate = DateTime.Now.AddDays(-5),
                    Status = OrderStatus.Completed,
                    SubtotalAmount = 4990000m,
                    Discount = 500000m,
                    TotalAmount = 4490000m,
                    Notes = "Khuyến mãi sinh nhật shop",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 19, ProductName = "Samsung Galaxy A15", UnitPrice = 4990000m, Quantity = 1, SubTotal = 4990000m }
                    }
                },

                // Orders từ 3 ngày trước
                new OrderDto
                {
                    OrderId = 5,
                    CustomerId = 4,
                    CustomerName = "Lê Thị D",
                    UserId = 1,
                    UserFullName = "Nguyễn Khắc Vượng",
                    OrderDate = DateTime.Now.AddDays(-3),
                    Status = OrderStatus.Returned,
                    SubtotalAmount = 19990000m,
                    Discount = 0m,
                    TotalAmount = 19990000m,
                    Notes = "Khách trả hàng - lỗi màn hình, đã hoàn tiền",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 13, ProductName = "Samsung Galaxy S24", UnitPrice = 19990000m, Quantity = 1, SubTotal = 19990000m }
                    }
                },
                new OrderDto
                {
                    OrderId = 6,
                    CustomerId = 5,
                    CustomerName = "Vũ Văn E",
                    UserId = 2,
                    UserFullName = "Nguyễn Phúc Hậu",
                    OrderDate = DateTime.Now.AddDays(-3),
                    Status = OrderStatus.Completed,
                    SubtotalAmount = 11980000m,
                    Discount = 1000000m,
                    TotalAmount = 10980000m,
                    Notes = "Sinh viên - áp dụng giảm giá 10%",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 7, ProductName = "iPhone SE (2022)", UnitPrice = 10990000m, Quantity = 1, SubTotal = 10990000m },
                        new OrderDetailDto { ProductId = 25, ProductName = "Redmi Note 13", UnitPrice = 990000m, Quantity = 1, SubTotal = 990000m }
                    }
                },

                // Orders từ 2 ngày trước
                new OrderDto
                {
                    OrderId = 7,
                    CustomerId = 6,
                    CustomerName = "Ngô Thị F",
                    UserId = 1,
                    UserFullName = "Nguyễn Khắc Vượng",
                    OrderDate = DateTime.Now.AddDays(-2),
                    Status = OrderStatus.Cancelled,
                    SubtotalAmount = 40990000m,
                    Discount = 0m,
                    TotalAmount = 40990000m,
                    Notes = "Khách hủy - đổi ý mua máy khác",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 14, ProductName = "Samsung Galaxy Z Fold5", UnitPrice = 40990000m, Quantity = 1, SubTotal = 40990000m }
                    }
                },
                new OrderDto
                {
                    OrderId = 8,
                    CustomerId = 7,
                    CustomerName = "Đặng Văn G",
                    UserId = 2,
                    UserFullName = "Nguyễn Phúc Hậu",
                    OrderDate = DateTime.Now.AddDays(-2),
                    Status = OrderStatus.Processing,
                    SubtotalAmount = 24990000m,
                    Discount = 1000000m,
                    TotalAmount = 23990000m,
                    Notes = "Đang chờ kho xuất hàng",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 22, ProductName = "Xiaomi 14 Ultra", UnitPrice = 24990000m, Quantity = 1, SubTotal = 24990000m }
                    }
                },

                // Orders từ hôm qua
                new OrderDto
                {
                    OrderId = 9,
                    CustomerId = null,
                    CustomerName = "Khách lẻ",
                    UserId = 1,
                    UserFullName = "Nguyễn Khắc Vượng",
                    OrderDate = DateTime.Now.AddDays(-1),
                    Status = OrderStatus.Completed,
                    SubtotalAmount = 29980000m,
                    Discount = 0m,
                    TotalAmount = 29980000m,
                    Notes = "Thanh toán tiền mặt",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 21, ProductName = "Xiaomi 14T", UnitPrice = 14990000m, Quantity = 2, SubTotal = 29980000m }
                    }
                },
                new OrderDto
                {
                    OrderId = 10,
                    CustomerId = 8,
                    CustomerName = "Phan Thị H",
                    UserId = 2,
                    UserFullName = "Nguyễn Phúc Hậu",
                    OrderDate = DateTime.Now.AddDays(-1),
                    Status = OrderStatus.Processing,
                    SubtotalAmount = 32970000m,
                    Discount = 3000000m,
                    TotalAmount = 29970000m,
                    Notes = "Mua theo nhóm, thanh toán 50% trước",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 28, ProductName = "POCO X6 Pro", UnitPrice = 10990000m, Quantity = 3, SubTotal = 32970000m }
                    }
                },

                // Orders hôm nay
                new OrderDto
                {
                    OrderId = 11,
                    CustomerId = 9,
                    CustomerName = "Trương Minh I",
                    UserId = 1,
                    UserFullName = "Nguyễn Khắc Vượng",
                    OrderDate = DateTime.Now.AddHours(-5),
                    Status = OrderStatus.Pending,
                    SubtotalAmount = 83970000m,
                    Discount = 5000000m,
                    TotalAmount = 78970000m,
                    Notes = "Khách VIP - Chờ xác nhận chuyển khoản",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 2, ProductName = "iPhone 15 Pro Max", UnitPrice = 33990000m, Quantity = 1, SubTotal = 33990000m },
                        new OrderDetailDto { ProductId = 14, ProductName = "Samsung Galaxy Z Fold5", UnitPrice = 40990000m, Quantity = 1, SubTotal = 40990000m },
                        new OrderDetailDto { ProductId = 21, ProductName = "Xiaomi 14T", UnitPrice = 8990000m, Quantity = 1, SubTotal = 8990000m }
                    }
                },
                new OrderDto
                {
                    OrderId = 12,
                    CustomerId = null,
                    CustomerName = "Khách lẻ",
                    UserId = 2,
                    UserFullName = "Nguyễn Phúc Hậu",
                    OrderDate = DateTime.Now.AddHours(-3),
                    Status = OrderStatus.Pending,
                    SubtotalAmount = 3490000m,
                    Discount = 0m,
                    TotalAmount = 3490000m,
                    Notes = "Mua tại cửa hàng",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 29, ProductName = "Redmi 13C", UnitPrice = 3490000m, Quantity = 1, SubTotal = 3490000m }
                    }
                },
                new OrderDto
                {
                    OrderId = 13,
                    CustomerId = 10,
                    CustomerName = "Bùi Thị J",
                    UserId = 1,
                    UserFullName = "Nguyễn Khắc Vượng",
                    OrderDate = DateTime.Now.AddHours(-2),
                    Status = OrderStatus.Processing,
                    SubtotalAmount = 71970000m,
                    Discount = 4000000m,
                    TotalAmount = 67970000m,
                    Notes = "Đơn hàng lớn - giao tận nơi, miễn phí vận chuyển",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 1, ProductName = "iPhone 15 Pro", UnitPrice = 28990000m, Quantity = 1, SubTotal = 28990000m },
                        new OrderDetailDto { ProductId = 11, ProductName = "Samsung Galaxy S24 Ultra", UnitPrice = 25990000m, Quantity = 1, SubTotal = 25990000m },
                        new OrderDetailDto { ProductId = 23, ProductName = "Xiaomi 13T Pro", UnitPrice = 16990000m, Quantity = 1, SubTotal = 16990000m }
                    }
                },
                new OrderDto
                {
                    OrderId = 14,
                    CustomerId = 1,
                    CustomerName = "Nguyễn Văn A",
                    UserId = 2,
                    UserFullName = "Nguyễn Phúc Hậu",
                    OrderDate = DateTime.Now.AddHours(-1),
                    Status = OrderStatus.Pending,
                    SubtotalAmount = 17990000m,
                    Discount = 500000m,
                    TotalAmount = 17490000m,
                    Notes = "Khách hàng quen - ưu tiên xử lý",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 6, ProductName = "iPhone 13", UnitPrice = 17990000m, Quantity = 1, SubTotal = 17990000m }
                    }
                },
                new OrderDto
                {
                    OrderId = 15,
                    CustomerId = null,
                    CustomerName = "Khách lẻ",
                    UserId = 1,
                    UserFullName = "Nguyễn Khắc Vượng",
                    OrderDate = DateTime.Now.AddMinutes(-30),
                    Status = OrderStatus.Pending,
                    SubtotalAmount = 19980000m,
                    Discount = 0m,
                    TotalAmount = 19980000m,
                    Notes = "Thanh toán tiền mặt tại shop",
                    Details = new List<OrderDetailDto>
                    {
                        new OrderDetailDto { ProductId = 16, ProductName = "Samsung Galaxy A55", UnitPrice = 9990000m, Quantity = 2, SubTotal = 19980000m }
                    }
                }
            };
        }

        public Task<ResponseWrapper<PagingResponse<OrderDto>>> GetOrdersAsync(OrderQueryDto query)
        {
            IEnumerable<OrderDto> result = _mockOrders;

            // Lọc theo trạng thái
            if (query.Status.HasValue)
            {
                result = result.Where(o => o.Status == query.Status.Value);
            }

            // Lọc theo khoảng thời gian
            if (query.OrderDate != null)
            {
                if (query.OrderDate.StartDate.HasValue)
                {
                    result = result.Where(o => o.OrderDate >= query.OrderDate.StartDate.Value);
                }

                if (query.OrderDate.EndDate.HasValue)
                {
                    // Thêm 1 ngày để bao gồm cả ngày kết thúc
                    var endDate = query.OrderDate.EndDate.Value.AddDays(1);
                    result = result.Where(o => o.OrderDate < endDate);
                }
            }

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrWhiteSpace(query.CustomerKeyword))
            {
                var keyword = query.CustomerKeyword.Trim().ToLower();
                result = result.Where(o =>
                    (o.CustomerName?.ToLower().Contains(keyword) ?? false) ||
                    (o.UserFullName?.ToLower().Contains(keyword) ?? false) ||
                    (o.Notes?.ToLower().Contains(keyword) ?? false) ||
                    o.OrderId.ToString().Contains(keyword) ||
                    o.Details.Any(d => d.ProductName.ToLower().Contains(keyword))
                );
            }

            // Sắp xếp
            if (query.Sorting != null && !string.IsNullOrWhiteSpace(query.Sorting.SortBy))
            {
                result = query.Sorting.SortBy.ToLower() switch
                {
                    "date" => query.Sorting.Desc
                        ? result.OrderByDescending(o => o.OrderDate)
                        : result.OrderBy(o => o.OrderDate),
                    "total" => query.Sorting.Desc
                        ? result.OrderByDescending(o => o.TotalAmount)
                        : result.OrderBy(o => o.TotalAmount),
                    "customer" => query.Sorting.Desc
                        ? result.OrderByDescending(o => o.CustomerName)
                        : result.OrderBy(o => o.CustomerName),
                    "status" => query.Sorting.Desc
                        ? result.OrderByDescending(o => o.Status)
                        : result.OrderBy(o => o.Status),
                    _ => result.OrderByDescending(o => o.OrderDate)
                };
            }
            else
            {
                // Mặc định sắp xếp theo ngày mới nhất
                result = result.OrderByDescending(o => o.OrderDate);
            }

            var totalCount = result.Count();
            
            // Phân trang
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
            
            var pagedResult = result
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagingResponse = new PagingResponse<OrderDto>
            {
                Items = pagedResult,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            var response = new ResponseWrapper<PagingResponse<OrderDto>>
            {
                Success = true,
                Message = "Orders retrieved successfully",
                Data = pagingResponse
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
            var details = (dto.Items ?? Array.Empty<OrderCreateItemDto>())
                .Select(i => new OrderDetailDto
                {
                    ProductId = i.ProductId,
                    ProductName = $"Product {i.ProductId}", // Mock product name
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    SubTotal = i.UnitPrice * i.Quantity
                })
                .ToList();

            var subtotal = details.Sum(d => d.SubTotal);
            var total = subtotal - dto.Discount;

            var order = new OrderDto
            {
                OrderId = _nextOrderId++,
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerId.HasValue ? $"Customer {dto.CustomerId}" : "Khách lẻ",
                UserId = 1,
                UserFullName = "Nguyễn Khắc Vượng",
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
    }
}
