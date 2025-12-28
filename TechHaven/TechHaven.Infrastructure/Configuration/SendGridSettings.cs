namespace TechHaven.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for SendGrid email service.
/// </summary>
public class SendGridSettings
{
  /// <summary>
  /// SendGrid API Key
  /// </summary>
  public string ApiKey { get; set; } = string.Empty;

  /// <summary>
  /// Sender email address (must be verified in SendGrid)
  /// </summary>
  public string SenderEmail { get; set; } = string.Empty;

  /// <summary>
  /// Sender display name
  /// </summary>
  public string SenderName { get; set; } = string.Empty;
}