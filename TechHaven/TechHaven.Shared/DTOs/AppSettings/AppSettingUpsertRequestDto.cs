namespace TechHaven.Shared.DTOs.AppSettings;

public class AppSettingUpsertRequestDto
{
	public string Key { get; set; } = string.Empty;

	public string? Value { get; set; }

	public SettingType ValueType { get; set; } = SettingType.String;

	public string? Category { get; set; }

	public string? Description { get; set; }

	public bool IsSystem { get; set; } = false;
}