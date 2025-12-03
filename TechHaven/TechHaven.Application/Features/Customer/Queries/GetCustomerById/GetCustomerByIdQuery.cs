using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Customers;
using TechHaven.Domain.Common;

namespace TechHaven.Application.Features.Customer.Queries.GetCustomerById;

public record GetCustomerByIdQuery(int CustomerId) : IQuery<Result<CustomerDto>>;