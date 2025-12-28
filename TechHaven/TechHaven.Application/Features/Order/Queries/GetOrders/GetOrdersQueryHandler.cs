using AutoMapper;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.SearchCriteria;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Orders;

namespace TechHaven.Application.Features.Order.Queries.GetOrders;

public class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, Result<PagingResponse<OrderDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOrdersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagingResponse<OrderDto>>> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var input = request.Filter;

        // 1. Chuyển đổi từ DTO (Layer ngoài) sang Criteria (Domain Layer)
        // Lý do: Repository chỉ hiểu Criteria, không nên để Repository phụ thuộc vào DTO
        var criteria = new OrderSearchCriteria
        {
            // Paging (Lấy từ class cha PagingRequest)
            PageNumber = input.PageNumber <= 0 ? 1 : input.PageNumber,
            PageSize = input.PageSize <= 0 ? 10 : input.PageSize,

            // Filter logic
            Status = (Domain.Enums.OrderStatus?)input.Status,
            CustomerKeyword = input.CustomerKeyword,

            
            // Ép kiểu date sang UTC trước khi đưa vào Criteria
            FromDate = input.OrderDate?.StartDate.HasValue == true
                ? DateTime.SpecifyKind(input.OrderDate.StartDate.Value, DateTimeKind.Utc)
                : null,

            ToDate = input.OrderDate?.EndDate.HasValue == true
                ? DateTime.SpecifyKind(input.OrderDate.EndDate.Value, DateTimeKind.Utc)
                : null,
            // -------------------------------
        };

        // 2. Xử lý Sorting
        if (
            input.Sorting != null 
            && !string.IsNullOrWhiteSpace(input.Sorting.SortBy)
            )
        {
            criteria.SortBy = input.Sorting.SortBy;
            criteria.SortDescending = input.Sorting.Desc;
        }
        else
        {
            // Mặc định: Sort theo ID giảm dần (Mới nhất lên đầu)
            criteria.SortBy = "OrderId"; //Ko có sort theo OrderId nhưng default sẽ sort theo OrderDate - Desc
            criteria.SortDescending = true;
        }

        // 3. Gọi Repository
        // Hàm này trả về Tuple (List<Order>, int TotalCount)
        var (orders, totalCount) = await _unitOfWork.Orders.SearchWithPaginationAsync(criteria, cancellationToken);

        // 4. Map Entity -> DTO
        var orderDtos = _mapper.Map<List<OrderDto>>(orders);

        // 5. Đóng gói PagingResponse
        var response = new PagingResponse<OrderDto>
        {
            Items = orderDtos,
            TotalCount = totalCount,
            PageNumber = criteria.PageNumber,
            PageSize = criteria.PageSize
        };

        return Result<PagingResponse<OrderDto>>.Success(response);
    }
}