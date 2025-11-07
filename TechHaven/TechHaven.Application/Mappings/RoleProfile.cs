using AutoMapper;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Roles;

namespace TechHaven.Application.Mappings;

public class RoleProfile : Profile
{
	public RoleProfile()
	{
		CreateMap<Role, RoleDto>();

		CreateMap<RoleCreateUpdateDto, Role>()
			.ForMember(dest => dest.RoleId, opt => opt.Ignore())
			.ForMember(dest => dest.Users, opt => opt.Ignore());
	}
}