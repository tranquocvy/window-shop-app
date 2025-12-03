using AutoMapper;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Customers;
using TechHaven.Application.Features.Customer.Commands.CreateCustomer;
using TechHaven.Application.Features.Customer.Commands.UpdateCustomer;

namespace TechHaven.Application.Mappings;

public class CustomerProfile : Profile
{
	public CustomerProfile()
	{
		// Entity -> DTO
		CreateMap<Customer, CustomerDto>()
			.ForMember(dest => dest.Type, opt => opt.MapFrom(src => (CustomerType)src.Type));

		// DTO -> Entity (for direct mapping)
		CreateMap<CustomerUpsertRequestDto, Customer>()
			.ForMember(dest => dest.CustomerId, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.Orders, opt => opt.Ignore())
			.ForMember(dest => dest.Type, opt => opt.MapFrom(src => (Domain.Enums.CustomerType)src.Type));

		// CreateCustomerCommand -> Entity
		CreateMap<CreateCustomerCommand, Customer>()
			.ForMember(dest => dest.CustomerId, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.Orders, opt => opt.Ignore())
			.ForMember(dest => dest.Type, opt => opt.MapFrom(src => (Domain.Enums.CustomerType)src.Type));

		// UpdateCustomerCommand -> Entity
		CreateMap<UpdateCustomerCommand, Customer>()
			.ForMember(dest => dest.CustomerId, opt => opt.Ignore()) // Giữ nguyên ID
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Giữ nguyên created date
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // Sẽ set riêng trong handler
			.ForMember(dest => dest.Orders, opt => opt.Ignore()) // Không update orders
			.ForMember(dest => dest.Type, opt => opt.MapFrom(src => (Domain.Enums.CustomerType)src.Type));
	}
}