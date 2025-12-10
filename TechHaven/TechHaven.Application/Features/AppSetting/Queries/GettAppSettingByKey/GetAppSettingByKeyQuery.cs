using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.AppSettings;

namespace TechHaven.Application.Features.AppSetting.Queries.GetAppSettingByKey;

public record GetAppSettingByKeyQuery(string Key) : IQuery<Result<AppSettingDto>>
{
    public int CurrentUserId { get; set; }
}