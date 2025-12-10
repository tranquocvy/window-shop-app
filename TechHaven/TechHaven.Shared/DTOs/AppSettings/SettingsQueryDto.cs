using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Shared.DTOs.AppSettings;

public class AppSettingsQueryDto : PagingRequest
{
  public string? SearchKeyword { get; set; }
}