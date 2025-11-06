using AutoMapper;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.AppSettings;

namespace TechHaven.Application.Mappings;

public class AppSettingProfile : Profile
{
	public AppSettingProfile()
	{
		CreateMap<AppSetting, AppSettingDto>();

		CreateMap<AppSettingCreateUpdateDto, AppSetting>()
			.ForMember(dest => dest.AppSettingId, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
	}
}