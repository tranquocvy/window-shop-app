using AutoMapper;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Customers;

namespace TechHaven.Application.Mappings;

public class CustomerProfile : Profile
{
	public CustomerProfile()
	{
		CreateMap<Customer, CustomerDto>();

		CreateMap<CustomerUpsertRequestDto, Customer>()
			.ForMember(dest => dest.CustomerId, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.Orders, opt => opt.Ignore());
	}
}