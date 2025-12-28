namespace TechHaven.Shared.DTOs.AppSettings;

public class AppSettingDto
{
		public int AppSettingId { get; set; }

		public string Key { get; set; } = string.Empty;

		public string? Value { get; set; }

		public SettingType ValueType { get; set; }

		public string? Category { get; set; }

		public string? Description { get; set; }

	public bool IsSystem { get; set; }

	public int? UserId { get; set; }

	public DateTime UpdatedAt { get; set; }
}

public enum SettingType
{
	String = 0,
	Number = 1,
	Decimal = 2,
	Bool = 3,
	Json = 4
}


