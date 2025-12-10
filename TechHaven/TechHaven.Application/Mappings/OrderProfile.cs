using AutoMapper;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Orders;

namespace TechHaven.Application.Mappings;

public class OrderProfile : Profile
{
	public OrderProfile()
	{
		CreateMap<OrderDetail, OrderDetailDto>()
			.ForMember(dest => dest.ProductName,
				opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : string.Empty))
			.ForMember(dest => dest.SubTotal,
				opt => opt.MapFrom(src => src.SubTotal));

		CreateMap<Order, OrderDto>()
			.ForMember(dest => dest.CustomerName,
				opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustomerName : null))
			.ForMember(dest => dest.UserFullName,
				opt => opt.MapFrom(src => src.User != null ? src.User.UserFullName : null))
			.ForMember(dest => dest.Details,
				opt => opt.MapFrom(src => src.OrderDetails != null ? src.OrderDetails : Array.Empty<OrderDetail>()))
            //Calculate TotalItems 
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(
					src => src.OrderDetails != null ? src.OrderDetails.Sum(x => x.Quantity) : 0)
						)
            .AfterMap((src, dest) =>
			{
				// Ensure Details is never null
				if (dest.Details == null)
				{
					dest.Details = Array.Empty<OrderDetailDto>();
				}
			});

		CreateMap<OrderUpsertItemDto, OrderDetail>()
			.ForMember(dest => dest.OrderDetailId, opt => opt.Ignore())
			.ForMember(dest => dest.OrderId, opt => opt.Ignore())
			.ForMember(dest => dest.Order, opt => opt.Ignore())
			.ForMember(dest => dest.Product, opt => opt.Ignore());

		CreateMap<OrderUpsertRequestDto, Order>()
			.ForMember(dest => dest.UserId, opt => opt.Ignore())
			.ForMember(dest => dest.OrderDate, opt => opt.Ignore())
			.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
			.ForMember(dest => dest.SubtotalAmount, opt => opt.Ignore())
			.ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
			.ForMember(dest => dest.User, opt => opt.Ignore())
			.ForMember(dest => dest.Customer, opt => opt.Ignore())
			.ForMember(dest => dest.Payments, opt => opt.Ignore())
			.ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.Items));
	}
}