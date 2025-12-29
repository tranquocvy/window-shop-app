namespace TechHaven.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for OpenAI API
/// </summary>
public class OpenAISettings
{
    /// <summary>
  /// OpenAI API Key
  /// </summary>
  public string ApiKey { get; set; } = string.Empty;

   /// <summary>
  /// Model ID 
  /// Recommended: "gpt-4o-mini" (cheap, fast, good for most tasks)
  /// Alternative: "gpt-4o" (more capable, more expensive)
  /// </summary>
  public string ModelId { get; set; } = string.Empty;

  public string? Endpoint { get; set; }

  /// <summary>
  /// Optional: Organization ID
  /// </summary>
  public string? OrganizationId { get; set; }
}