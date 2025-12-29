using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.AI;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Presentation.WebAPI.Controllers;

// [Authorize]
public class AIController : BaseApiController
{
  private readonly IAIChatService _aiChatService;
  private readonly ILogger<AIController> _logger;

  public AIController(
    IAIChatService aiChatService,
    ILogger<AIController> logger)
  {
    _aiChatService = aiChatService;
    _logger = logger;
  }

  /// <summary>
  /// Chat với AI Assistant
  /// </summary>
  [HttpPost("chat")]
  [ProducesResponseType(typeof(ResponseWrapper<ChatResponseDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Chat(
    [FromBody] ChatRequestDto request,
    CancellationToken cancellationToken)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.Message))
      {
        return BadRequest(new ResponseWrapper<object>
        {
          Success = false,
          Message = "Message is required"
        });
      }

      _logger.LogInformation("AI Chat request from user: {Message}", request.Message);

      // Convert history if provided
      var history = request.History?.Select(h =>
        new ChatMessageContent(
          h.Role == Role.User
            ? AuthorRole.User
            : AuthorRole.Assistant,
          h.Content
        )).ToList();

      var aiResponse = await _aiChatService.ChatAsync(request.Message, history);

      return Ok(new ResponseWrapper<ChatResponseDto>
      {
        Success = true,
        Message = "AI response generated successfully",
        Data = new ChatResponseDto
        {
          Response = aiResponse,
          Timestamp = DateTime.UtcNow
        }
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error processing AI chat");
      return BadRequest(new ResponseWrapper<object>
      {
        Success = false,
        Message = "Failed to process chat",
        Errors = new List<string> { ex.Message }
      });
    }
  }
}