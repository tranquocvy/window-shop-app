namespace TechHaven.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for Supabase Storage service.
/// </summary>
public class SupabaseSettings
{
  public string Url { get; set; } = string.Empty;
  public string ApiKey { get; set; } = string.Empty;
  public string BucketName { get; set; } = string.Empty;
  public long MaxFileSizeBytes { get; set; } = 5_242_880; // 5MB default
  public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".webp" };
}