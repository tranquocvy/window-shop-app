using AutoMapper;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Commissions;

namespace TechHaven.Application.Mappings;

public class CommissionProfile : Profile
{
	public CommissionProfile()
	{
		CreateMap<Commission, CommissionDto>()
			.ForMember(dest => dest.UserFullName,
				opt => opt.MapFrom(src => src.User != null ? src.User.UserFullName : null));

		CreateMap<CommissionCreateUpdateDto, Commission>()
			.ForMember(dest => dest.CommissionId, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.User, opt => opt.Ignore());
	}
}