using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using TechHaven.Application.Features.AI.Plugins;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Interfaces;
using TechHaven.Infrastructure.Configuration;

namespace TechHaven.Infrastructure.Services.AI;

public class GeminiAIChatService : IAIChatService
{
  private readonly Kernel _kernel;
  private readonly IChatCompletionService _chatService;
  private readonly RAGService _ragService;
  private readonly ILogger<GeminiAIChatService> _logger;
  private readonly string _modelId;

  public GeminiAIChatService(
      IOptions<GeminiSettings> geminiSettings,
      IUnitOfWork unitOfWork,
      RAGService ragService,
      ILogger<GeminiAIChatService> logger)
  {
    _ragService = ragService;
    _logger = logger;
    _modelId = geminiSettings.Value.ModelId;

    // Validate settings
    if (string.IsNullOrWhiteSpace(geminiSettings.Value.ModelId))
    {
      throw new InvalidOperationException("GeminiSettings.ModelId is required");
    }

    if (string.IsNullOrWhiteSpace(geminiSettings.Value.ApiKey))
    {
      throw new InvalidOperationException("GeminiSettings.ApiKey is required");
    }

    // Build Semantic Kernel
    var builder = Kernel.CreateBuilder();

    // Add Google Gemini connector
    builder.AddGoogleAIGeminiChatCompletion(
        modelId: geminiSettings.Value.ModelId,
        apiKey: geminiSettings.Value.ApiKey);

    // Register plugins
    builder.Plugins.AddFromObject(new ProductPlugin(unitOfWork), "ProductPlugin");
    builder.Plugins.AddFromObject(new OrderPlugin(unitOfWork), "OrderPlugin");

    _kernel = builder.Build();
    _chatService = _kernel.GetRequiredService<IChatCompletionService>();

    _logger.LogInformation(
        "AI Chat Service initialized with model: {ModelId}",
        geminiSettings.Value.ModelId);
  }

  public async Task<string> ChatAsync(
      string userMessage,
      List<ChatMessageContent>? history = null)
  {
    const int maxRetries = 3;
    const int delayMs = 1000;

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
      try
      {
        _logger.LogInformation(
            "User message (attempt {Attempt}/{MaxRetries}): {Message}",
            attempt,
            maxRetries,
            userMessage);

        // Build chat history
        var chatHistory = new ChatHistory();

        // Add system context (RAG)
        var systemContext = _ragService.BuildFullContext();
        chatHistory.AddSystemMessage(systemContext);

        // Add conversation history if provided
        if (history != null)
        {
          foreach (var msg in history)
          {
            chatHistory.Add(msg);
          }
        }

        // Add current user message
        chatHistory.AddUserMessage(userMessage);

        // Configure execution settings for function calling
        var executionSettings = new GeminiPromptExecutionSettings
        {
          MaxTokens = 2000,
          Temperature = 0.7,
          ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions
        };

        // Get AI response with function calling
        var response = await _chatService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            _kernel);

        var aiResponse = response.Content ?? "Xin lỗi, tôi không thể trả lời câu hỏi này.";

        _logger.LogInformation("AI response: {Response}", aiResponse);

        return aiResponse;
      }
      catch (HttpOperationException ex) when (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
      {
        _logger.LogWarning(
            ex,
            "Gemini API unavailable (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}ms...",
            attempt,
            maxRetries,
            delayMs * attempt);

        if (attempt < maxRetries)
        {
          await Task.Delay(delayMs * attempt);
          continue;
        }

        _logger.LogError(ex, "Gemini API unavailable after {MaxRetries} attempts", maxRetries);
        return "Xin lỗi, dịch vụ AI đang tạm thời quá tải. Vui lòng thử lại sau vài giây.";
      }
      catch (HttpOperationException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
      {
        _logger.LogWarning(ex, "Rate limit exceeded. Model: {ModelId}", _modelId);
        return "Xin lỗi, bạn đã gửi quá nhiều yêu cầu. Vui lòng thử lại sau 1 phút.";
      }
      catch (HttpOperationException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
      {
        _logger.LogError(ex, "Invalid request to Gemini API. Model: {ModelId}", _modelId);
        return "Xin lỗi, yêu cầu không hợp lệ. Vui lòng thử lại với câu hỏi khác.";
      }
      catch (HttpOperationException ex)
      {
        _logger.LogError(
            ex,
            "HTTP error from Gemini API. Status: {StatusCode}, Model: {ModelId}",
            ex.StatusCode,
            _modelId);
        return $"Xin lỗi, đã có lỗi kết nối với dịch vụ AI (Mã lỗi: {ex.StatusCode}). Vui lòng thử lại sau.";
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Unexpected error in AI chat. Model: {ModelId}", _modelId);
        return "Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại sau.";
      }
    }

    return "Xin lỗi, không thể kết nối với dịch vụ AI. Vui lòng thử lại sau.";
  }
}