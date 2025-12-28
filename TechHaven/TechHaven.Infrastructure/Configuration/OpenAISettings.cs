namespace TechHaven.Infrastructure.Configuration;

public class OpenAISettings
{
  public string ApiKey { get; set; } = string.Empty;
  public string ModelId { get; set; } = string.Empty;
  public string? Endpoint { get; set; }
}