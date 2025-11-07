using AutoMapper;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Payments;

namespace TechHaven.Application.Mappings;

public class PaymentProfile : Profile
{
	public PaymentProfile()
	{
		CreateMap<Payment, PaymentDto>();

		CreateMap<PaymentCreateDto, Payment>()
			.ForMember(dest => dest.PaymentId, opt => opt.Ignore())
			.ForMember(dest => dest.PaymentDate, opt => opt.Ignore())
			.ForMember(dest => dest.Order, opt => opt.Ignore());
	}
}