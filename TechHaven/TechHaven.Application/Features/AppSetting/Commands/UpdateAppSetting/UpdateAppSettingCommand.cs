using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Enums;
using TechHaven.Shared.DTOs.AppSettings;

namespace TechHaven.Application.Features.AppSetting.Commands.UpdateAppSetting;

public record UpdateAppSettingCommand : ICommand<Result<AppSettingDto>>
{
    public int CurrentUserId { get; set; } //user context
    public string Key { get; init; } = string.Empty;
    public string? Value { get; init; }
    public string? Category { get; init; }
    public string? Description { get; init; }

    // Tùy chọn: Có cho phép đổi kiểu dữ liệu không? Thường là KHÔNG nên đổi runtime, nhưng nếu cần thiết thì cứ để.
    // public SettingType ValueType { get; init; } 

    public bool IsSystem { get; init; }
}