namespace TechHaven.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for Google Gemini AI
/// </summary>
public class GeminiSettings
{
  public string ApiKey { get; set; } = string.Empty;
  public string ModelId { get; set; } = string.Empty;
}