namespace TechHaven.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for Brevo (Sendinblue) email service.
/// </summary>
public class BrevoSettings
{
  /// <summary>
  /// Brevo API Key (v3-...)
  /// </summary>
  public string ApiKey { get; set; } = string.Empty;

  /// <summary>
  /// Sender email address (must be verified in Brevo)
  /// </summary>
  public string SenderEmail { get; set; } = string.Empty;

  /// <summary>
  /// Sender display name
  /// </summary>
  public string SenderName { get; set; } = string.Empty;
}