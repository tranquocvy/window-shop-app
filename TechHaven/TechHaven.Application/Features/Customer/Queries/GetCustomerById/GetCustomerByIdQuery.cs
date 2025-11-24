using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Customers;

namespace TechHaven.Application.Features.Customer.Queries.GetCustomerById;

public record GetCustomerByIdQuery(int CustomerId) : IQuery<CustomerDto>;