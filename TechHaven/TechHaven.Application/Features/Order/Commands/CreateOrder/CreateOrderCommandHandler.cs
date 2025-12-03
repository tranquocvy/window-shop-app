using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Orders; // for OrderDto, OrderDetailDto
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.Common;      //  Result, ErrorType
using TechHaven.Domain.Enums;       
using AutoMapper;
using TechHaven.Domain.Entities;
using OrderStatus = TechHaven.Domain.Enums.OrderStatus;    // use entity Order, Product...

namespace TechHaven.Application.Features.Order.Commands.CreateOrder;

public class CreateOrderCommandHandler
    : ICommandHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto>> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validate Customer 
            if (request.CustomerId.HasValue)
            {
                var customerExists = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId.Value);
                if (customerExists == null)
                {
                    return Result<OrderDto>.Failure(
                        $"Customer with ID {request.CustomerId} not found.",
                        ErrorType.NotFound);
                }
            }

            // 2. Declare Entity Order
            // Lưu ý: map các thông tin cơ bản, nhưng sẽ tính toán lại tiền nong
            var order = new Domain.Entities.Order
            {
                CustomerId = request.CustomerId,
                UserId = request.UserId, // Lấy từ Token do Controller truyền vào
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending, // Mặc định là Pending
                Notes = request.Notes,
                Discount = request.Discount, // Giả sử discount client gửi là hợp lệ (hoặc cần validate thêm logic voucher)
                OrderDetails = new List<OrderDetail>(), // ở entity thì là OrderDetails
            };

            decimal calculatedSubTotal = 0;

            // 3. Xử lý từng sản phẩm (Critical Logic)
            foreach (var itemDto in request.Details) //ở command thì là Details thay vì OrderDetails như entity
            {
                // Lấy thông tin sản phẩm từ DB để đảm bảo giá đúng và check tồn kho
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);

                if (product == null)
                {
                    return Result<OrderDto>.Failure(
                        $"Product with ID {itemDto.ProductId} not found.",
                        ErrorType.NotFound);
                }

                // Check tồn kho
                if (product.StockQuantity < itemDto.Quantity)
                {
                    return Result<OrderDto>.Failure(
                        $"Product '{product.ProductName}' has insufficient stock. Available: {product.StockQuantity}, Requested: {itemDto.Quantity}",
                        ErrorType.Validation);
                }

                // Trừ tồn kho
                product.StockQuantity -= itemDto.Quantity;
                // Nếu cần update Product thì gọi Update, nhưng thường EF Core tracking tự nhận biết thay đổi
                // await _unitOfWork.Products.UpdateAsync(product); // Tùy vào cách implement GenericRepository 

                // Tạo OrderDetail với giá lấy từ DB (Product.SellPrice)
                // TUYỆT ĐỐI KHÔNG dùng itemDto.UnitPrice từ client gửi lên để tính tiền
                var orderDetail = new OrderDetail
                {
                    ProductId = product.ProductId, //product laf entity lay tu DB
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.SellPrice // Lấy giá bán hiện tại trong DB
                };

                order.OrderDetails.Add(orderDetail);

                // Cộng dồn SubTotal
                calculatedSubTotal += (orderDetail.UnitPrice * orderDetail.Quantity);
            } //end foreach

            // 4. Tính toán tổng tiền cuối cùng
            order.SubtotalAmount = calculatedSubTotal;
            //TODO: Discount tạm thời là %?
            order.TotalAmount = (order.SubtotalAmount - order.SubtotalAmount * order.Discount); 

            // Validation logic: Không để tổng tiền âm
            if (order.TotalAmount < 0) order.TotalAmount = 0;

            // 5. Lưu xuống DB
            await _unitOfWork.Orders.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 6. Map kết quả trả về
            var orderResponse = _mapper.Map<OrderDto>(order);

            orderResponse.Discount = order.Discount;

            return Result<OrderDto>.Success(orderResponse);
        }
        catch (Exception ex)
        {
            return Result<OrderDto>.Failure(
                $"Failed to create order: {ex.Message}",
                ErrorType.InternalError);
        }
    }
}