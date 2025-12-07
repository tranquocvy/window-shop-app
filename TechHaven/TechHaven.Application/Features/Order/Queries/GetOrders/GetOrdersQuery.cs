using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Orders;

namespace TechHaven.Application.Features.Order.Queries.GetOrders;

/// <summary>
/// Query take an OrderListQueryDto and returns a PagingResponse containing a list of OrderDto
/// </summary>
/// <param name="Filter">
///     use Filter as OrderListQueryDto (in DTOs ~ Application layer) instead of OrderSearchCriteria (Domain)
///     
///     Reason: (if we use OrderSearchCriteria)
///        - When API controller (in Presentation layer) delcare a query, it need to know the Filter - Order Search Criteria from Domain
///        - To gurantee the rule of Clean Architecture, Presentation layer can't depend directly on Domain layer
///        -> We use Order List QueryDto (Application layer) instead of Order Search Criteria (Domain layer)
/// 
/// </param>
public record GetOrdersQuery(OrderListQueryDto Filter) : IQuery<Result<PagingResponse<OrderDto>>>;