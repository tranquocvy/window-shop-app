using AutoMapper;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Orders;

namespace TechHaven.Application.Features.Order.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto>> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Gọi Repository
        var order = await _unitOfWork.Orders.GetWithDetailsAsync(request.OrderId, cancellationToken);

        // 2. Kiểm tra tồn tại
        if (order == null)
        {
            return Result<OrderDto>.Failure(
                $"Order with ID {request.OrderId} not found.",
                ErrorType.NotFound);
        }

        // 3. Map sang DTO
        var orderDto = _mapper.Map<OrderDto>(order);

        // 4. Trả về
        return Result<OrderDto>.Success(orderDto);
    }
}