using AutoMapper;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Mappings;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        // Map từ User entity sang LoginResponseDto
        CreateMap<User, LoginResponseDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : null))
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId));
    }
}