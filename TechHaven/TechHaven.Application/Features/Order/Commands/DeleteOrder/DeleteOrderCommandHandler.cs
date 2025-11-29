using TechHaven.Application.Interfaces;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Enums;

namespace TechHaven.Application.Features.Order.Commands.DeleteOrder;

public class DeleteOrderCommandHandler : ICommandHandler<DeleteOrderCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Lấy Order kèm theo chi tiết (Bắt buộc để biết đường hoàn kho)
            // Hàm này bạn đã verify ở bước trước, nó đã Include Product bên trong.
            var order = await _unitOfWork.Orders.GetWithDetailsAsync(request.OrderId, cancellationToken);

            if (order == null)
            {
                return Result<bool>.Failure($"Order {request.OrderId} not found.", ErrorType.NotFound);
            }

            // 2. [Business Rule] Kiểm tra trạng thái
            // Chỉ cho phép xóa khi đơn hàng còn ở trạng thái Pending hoặc Cancelled.
            // Nếu đơn hàng đang xử lý hoặc đã hoàn thành -> Không cho xóa (chỉ nên cho Cancel).
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Cancelled)
            {
                return Result<bool>.Failure(
                    $"Cannot delete order with status '{order.Status}'. Only Pending or Cancelled orders can be deleted.",
                    ErrorType.Validation);
            }

            // 3. Hoàn trả tồn kho (Restock Inventory)
            // Vì GetWithDetailsAsync đã Include("OrderDetails.Product"), nên product đã được tracking
            foreach (var item in order.OrderDetails)
            {
                // Kiểm tra null safety cho chắc chắn (dù logic DB đã ràng buộc)
                if (item.Product != null)
                {
                    item.Product.StockQuantity += item.Quantity;
                    // Không cần gọi UpdateAsync cho Product vì EF Core tự track thay đổi trên graph
                }
            }

            // 4. Xóa Order
            // Khi xóa Order, EF Core (với cấu hình Cascade Delete mặc định) sẽ xóa luôn OrderDetails
            await _unitOfWork.Orders.DeleteAsync(order, cancellationToken);

            // 5. Lưu thay đổi (Transaction sẽ commit cả việc cập nhật kho và xóa order)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                $"Failed to delete order: {ex.Message}",
                ErrorType.InternalError);
        }
    }
}