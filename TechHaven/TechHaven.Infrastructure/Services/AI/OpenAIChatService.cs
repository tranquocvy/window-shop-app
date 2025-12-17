using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TechHaven.Application.Features.AI.Plugins;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Interfaces;
using TechHaven.Infrastructure.Configuration;

namespace TechHaven.Infrastructure.Services;

public class OpenAIChatService : IAIChatService
{
  private readonly Kernel _kernel;
  private readonly IChatCompletionService _chatService;
  private readonly RAGService _ragService;
  private readonly ILogger<OpenAIChatService> _logger;
  private readonly string _modelId;

  public OpenAIChatService(
      IOptions<OpenAISettings> openAISettings,
      IUnitOfWork unitOfWork,
      RAGService ragService,
      ILogger<OpenAIChatService> logger)
  {
    _ragService = ragService;
    _logger = logger;
    _modelId = openAISettings.Value.ModelId;

    // Validate settings
    if (string.IsNullOrWhiteSpace(openAISettings.Value.ModelId))
    {
      throw new InvalidOperationException("OpenAISettings.ModelId is required");
    }

    if (string.IsNullOrWhiteSpace(openAISettings.Value.ApiKey))
    {
      throw new InvalidOperationException("OpenAISettings.ApiKey is required");
    }

    // Build Semantic Kernel
    var builder = Kernel.CreateBuilder();

    // Add OpenAI connector
    builder.AddOpenAIChatCompletion(
      modelId: _modelId,
      apiKey: openAISettings.Value.ApiKey,
      endpoint: new Uri(openAISettings.Value.Endpoint ?? string.Empty)
    );

    // Register plugins
    builder.Plugins.AddFromObject(new ProductPlugin(unitOfWork), "ProductPlugin");
    builder.Plugins.AddFromObject(new OrderPlugin(unitOfWork), "OrderPlugin");

    _kernel = builder.Build();
    _chatService = _kernel.GetRequiredService<IChatCompletionService>();

    _logger.LogInformation(
        "AI Chat Service initialized with OpenAI model: {ModelId}",
        openAISettings.Value.ModelId);
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
        var executionSettings = new OpenAIPromptExecutionSettings
        {
          MaxTokens = 2000,
          Temperature = 0.7,
          ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        // Get AI response with function calling
        var response = await _chatService.GetChatMessageContentAsync(
          chatHistory,
          executionSettings,
          _kernel
        );

        var aiResponse = response.Content ?? "Xin lỗi, tôi không thể trả lời câu hỏi này.";

        _logger.LogInformation("AI response: {Response}", aiResponse);

        return aiResponse;
      }
      catch (HttpOperationException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
      {
        _logger.LogWarning(ex, "Rate limit exceeded. Model: {ModelId}", _modelId);

        if (attempt < maxRetries)
        {
          await Task.Delay(delayMs * attempt * 2);
          continue;
        }

        return "Xin lỗi, bạn đã gửi quá nhiều yêu cầu. Vui lòng thử lại sau 1 phút.";
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error in AI chat. Model: {ModelId}", _modelId);

        if (attempt < maxRetries)
        {
          await Task.Delay(delayMs * attempt);
          continue;
        }

        return "Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại sau.";
      }
    }

    return "Xin lỗi, không thể kết nối với dịch vụ AI. Vui lòng thử lại sau.";
  }
}