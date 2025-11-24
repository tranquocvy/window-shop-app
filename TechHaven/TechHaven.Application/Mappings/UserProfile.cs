using AutoMapper;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Shared.DTOs.Users;

namespace TechHaven.Application.Mappings;

public class UserProfile : Profile
{
	public UserProfile()
	{
		// Mapping User -> UserDto
		CreateMap<User, UserDto>()
				.ForMember(dest => dest.RoleName,
						opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : string.Empty));

		// Mapping UserCreateUpdateDto -> User
		CreateMap<UserCreateUpdateDto, User>()
				.ForMember(dest => dest.UserId, opt => opt.Ignore())
				.ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
				.ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
				.ForMember(dest => dest.ActivatedAt, opt => opt.Ignore())
				.ForMember(dest => dest.Role, opt => opt.Ignore())
				.ForMember(dest => dest.Orders, opt => opt.Ignore())
				.ForMember(dest => dest.Commissions, opt => opt.Ignore());
	}
}