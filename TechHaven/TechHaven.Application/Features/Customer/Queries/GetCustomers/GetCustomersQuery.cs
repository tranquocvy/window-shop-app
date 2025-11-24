using TechHaven.Shared.DTOs.Customers;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.SearchCriteria;

namespace TechHaven.Application.Features.Customer.Queries.GetCustomers;

public record GetCustomersQuery(CustomerSearchCriteria criteria) : IQuery<PagingResponse<CustomerDto>>
{
}