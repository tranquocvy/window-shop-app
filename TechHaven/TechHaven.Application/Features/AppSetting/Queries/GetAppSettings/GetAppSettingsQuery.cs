using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.AppSettings;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Application.Features.AppSetting.Queries.GetAppSettings;

public class GetAppSettingsQuery : PagingRequest, IQuery<Result<PagingResponse<AppSettingDto>>>
{
    public int CurrentUserId { get; set; }//fron tok
    public string? SearchKeyword { get; set; }
}