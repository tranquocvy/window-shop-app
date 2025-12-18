using Microsoft.SemanticKernel;

namespace TechHaven.Application.Interfaces;

public interface IAIChatService
{
  Task<string> ChatAsync(string userMessage, List<ChatMessageContent>? history = null);
}