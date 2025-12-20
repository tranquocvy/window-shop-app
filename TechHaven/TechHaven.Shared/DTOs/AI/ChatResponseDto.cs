namespace TechHaven.Shared.DTOs.AI;

public class ChatResponseDto
{
  public string Response { get; set; } = string.Empty;
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}