using TechHaven.Shared.DTOs.Customers;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.SearchCriteria;
using TechHaven.Domain.Common;

namespace TechHaven.Application.Features.Customer.Queries.GetCustomers;

public record GetCustomersQuery(CustomerSearchCriteria criteria) : IQuery<Result<PagingResponse<CustomerDto>>>
{
}