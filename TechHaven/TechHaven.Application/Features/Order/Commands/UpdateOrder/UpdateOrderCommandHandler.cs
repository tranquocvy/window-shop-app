using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Orders;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Entities;
using AutoMapper;

namespace TechHaven.Application.Features.Order.Commands.UpdateOrder;

public class UpdateOrderCommandHandler : ICommandHandler<UpdateOrderCommand, Result<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {

            // 1. Lấy Order cũ lên, BẮT BUỘC phải Include OrderDetails
            // Giả sử Repository của bạn có hàm GetByIdWithDetailsAsync
            // Nếu dùng GenericRepository, bạn cần dùng Specification hoặc phương thức Queryable

            // Cách dùng Specification (Clean Architecture chuẩn):
            // var spec = new OrderWithDetailsSpecification(request.OrderId);
            // var order = await _unitOfWork.Orders.FirstOrDefaultAsync(spec);
            //TODO: Setup Specification cho Order Detail

            // Code tạm thời nếu bạn chưa setup xong Specification:
            var order = await _unitOfWork.Orders.GetWithDetailsAsync(request.OrderId);

            if (order == null)
            {
                return Result<OrderDto>.Failure($"Order {request.OrderId} not found", ErrorType.NotFound);
            }

            // 2. Update thông tin Header
            order.CustomerId = request.CustomerId;
            order.Status = request.Status;
            order.Notes = request.Notes;
            order.Discount = request.Discount;

            // 3. XỬ LÝ ORDER DETAILS & TỒN KHO (Phần khó nhất -- Quan trọng)

            // Danh sách ProductID user gửi lên
            var incomingProductIds = request.Details.Select(x => x.ProductId).ToList();

            // A. XÓA item cũ không còn trong danh sách mới
            // Những item đang có trong DB nhưng không có trong request -> Xóa
            var itemsToDelete = order.OrderDetails! // can not be null
                .Where(x => !incomingProductIds.Contains(x.ProductId))
                .ToList();

            foreach (var item in itemsToDelete)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                // Hoàn trả số lượng về kho
                if (product != null)
                {
                    product.StockQuantity += item.Quantity; // Cộng lại kho
                }

                // Xóa khỏi danh sách liên kết
                order.OrderDetails!.Remove(item);
            }

            // B. CẬP NHẬT hoặc THÊM MỚI
            decimal calculatedSubTotal = 0;

            foreach (var incomingItem in request.Details)
            {
                var existingItem = order.OrderDetails!
                    .FirstOrDefault(x => x.ProductId == incomingItem.ProductId);

                var product = await _unitOfWork.Products.GetByIdAsync(incomingItem.ProductId);
                if (product == null)
                    return Result<OrderDto>.Failure($"Product {incomingItem.ProductId} not found", ErrorType.NotFound);

                if (existingItem != null)
                {
                    // --- CASE: CẬP NHẬT SỐ LƯỢNG ---
                    int quantityDiff = incomingItem.Quantity - existingItem.Quantity;

                    if (quantityDiff > 0) // Mua thêm
                    {
                        if (product.StockQuantity < quantityDiff)
                        {
                            return Result<OrderDto>.Failure(
                                $"Insufficient stock for product '{product.ProductName}'. Need {quantityDiff} more.",
                                ErrorType.Validation);
                        }
                        product.StockQuantity -= quantityDiff; // Trừ kho
                    }
                    else if (quantityDiff < 0) // Mua ít đi
                    {
                        product.StockQuantity += Math.Abs(quantityDiff); // Trả lại kho
                    }

                    // Cập nhật item
                    existingItem.Quantity = incomingItem.Quantity;
                    existingItem.UnitPrice = product.SellPrice; // Cập nhật lại giá theo thời điểm hiện tại (Tùy nghiệp vụ)

                    calculatedSubTotal += existingItem.Quantity * existingItem.UnitPrice;
                }
                else
                {
                    // --- CASE: THÊM MỚI ITEM VÀO ORDER CŨ ---
                    if (product.StockQuantity < incomingItem.Quantity)
                    {
                        return Result<OrderDto>.Failure(
                             $"Insufficient stock for product '{product.ProductName}'.",
                             ErrorType.Validation);
                    }

                    product.StockQuantity -= incomingItem.Quantity; // Trừ kho

                    var newDetail = new OrderDetail
                    {
                        ProductId = product.ProductId,
                        Quantity = incomingItem.Quantity,
                        UnitPrice = product.SellPrice
                    };
                    order.OrderDetails!.Add(newDetail);

                    calculatedSubTotal += newDetail.Quantity * newDetail.UnitPrice;
                }
            }

            // 4. Tính lại tổng tiền
            order.SubtotalAmount = calculatedSubTotal;
            // Công thức mới: Total = Sub - (Sub * Discount)
            order.TotalAmount = order.SubtotalAmount - (order.SubtotalAmount * order.Discount);

            if (order.TotalAmount < 0) order.TotalAmount = 0;

            // 5. Save Changes
            // Do EF Core Tracking, ta chỉ cần gọi SaveChangesAsync
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 6. Return Result
            var orderDto = _mapper.Map<OrderDto>(order);
            return Result<OrderDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            return Result<OrderDto>.Failure($"Update failed: {ex.Message}", ErrorType.InternalError);
        }
    }
}