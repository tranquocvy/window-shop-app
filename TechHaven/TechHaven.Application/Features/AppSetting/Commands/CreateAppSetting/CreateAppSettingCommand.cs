using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Enums; // SettingType
using TechHaven.Shared.DTOs.AppSettings;
using SettingType = TechHaven.Domain.Enums.SettingType;

namespace TechHaven.Application.Features.AppSetting.Commands.CreateAppSetting;

/// <summary>
/// Represents a command to create an application setting.
/// </summary>
public record CreateAppSettingCommand : ICommand<Result<AppSettingDto>>
{
    public int CurrentUserId { get; set; }//user context
    public string Key { get; init; } = string.Empty;
    public string? Value { get; init; }
    public SettingType ValueType { get; init; }
    public string? Category { get; init; }
    public string? Description { get; init; }
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
    public bool IsSystem { get; set; }
}