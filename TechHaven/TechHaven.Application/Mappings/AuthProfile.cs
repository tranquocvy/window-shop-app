using AutoMapper;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Mappings;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        // KO NÊN:
        // CreateMap<User, LoginResponseDto>();

        // NÊN: mapping cho GetCurrentUser
        CreateMap<User, UserInfoDto>()
            .ForMember(dest => dest.RoleName, opt => opt.Ignore()); // Sẽ set riêng từ Role entity
    }
}