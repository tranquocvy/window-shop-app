namespace TechHaven.Shared.DTOs.AI;

public class ChatRequestDto
{
  public string Message { get; set; } = string.Empty;
  public List<ChatMessageDto>? History { get; set; }
}

public class ChatMessageDto
{
  public Role Role { get; set; }
  public string Content { get; set; } = string.Empty;
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum Role
{
  User = 1,
  Assistance = 2
}