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

  // Sliding Window: CHỈ giữ N tin nhắn gần nhất
  private const int MAX_HISTORY_MESSAGES = 5;

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
    builder.Plugins.AddFromObject(new DashboardPlugin(unitOfWork), "DashboardPlugin");

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

        // Build chat history with Sliding Window
        var chatHistory = BuildOptimizedChatHistory(userMessage, history);

        // Configure execution settings for function calling
        var executionSettings = new OpenAIPromptExecutionSettings
        {
          MaxTokens = 1000,
          Temperature = 0.7,
          ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
          FrequencyPenalty = 0.0,
          PresencePenalty = 0.0
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
        // Phân tích rate limit type từ response headers hoặc message
        var errorMessage = ex.Message?.ToLower() ?? string.Empty;
        var isRPDLimit = errorMessage.Contains("requests per day") ||
                         errorMessage.Contains("daily") ||
                         errorMessage.Contains("quota");

        if (isRPDLimit)
        {
          _logger.LogWarning(
              ex,
              "Daily request limit (RPD) exceeded. Model: {ModelId}",
              _modelId);

          return "Xin lỗi, hệ thống đã đạt giới hạn số lượng yêu cầu trong ngày.\n" +
                 "Vui lòng thử lại sau 24 giờ hoặc liên hệ quản trị viên.";
        }

        // Rate limit per minute
        _logger.LogWarning(
            ex,
            "Rate limit per minute exceeded (attempt {Attempt}/{MaxRetries}). Model: {ModelId}",
            attempt,
            maxRetries,
            _modelId);

        if (attempt < maxRetries)
        {
          var retryDelay = delayMs * attempt * 2; // Exponential backoff: 2s, 4s, 6s
          _logger.LogInformation(
              "Retrying in {Delay}ms...",
              retryDelay);

          await Task.Delay(retryDelay);
          continue;
        }

        return "Xin lỗi, bạn đã gửi quá nhiều yêu cầu liên tiếp.\n" +
               "Vui lòng thử lại sau 1-2 phút.";
      }
      catch (HttpOperationException ex) when (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
      {
        _logger.LogWarning(
            ex,
            "Service unavailable (attempt {Attempt}/{MaxRetries}). Model: {ModelId}",
            attempt,
            maxRetries,
            _modelId);

        if (attempt < maxRetries)
        {
          var retryDelay = delayMs * attempt * 3; // 3s, 6s, 9s
          await Task.Delay(retryDelay);
          continue;
        }

        return "Dịch vụ AI đang tạm thời bảo trì.\n" +
               "Vui lòng thử lại sau 5-10 phút.";
      }
      catch (HttpOperationException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
      {
        _logger.LogError(
            ex,
            "Unauthorized: Invalid API key. Model: {ModelId}",
            _modelId);

        return "Lỗi xác thực API key. Vui lòng liên hệ quản trị viên.";
      }
      catch (HttpOperationException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
      {
        _logger.LogError(
            ex,
            "Bad request: Invalid parameters. Model: {ModelId}",
            _modelId);

        return "Yêu cầu không hợp lệ. Vui lòng thử lại với câu hỏi khác.";
      }
      catch (HttpOperationException ex)
      {
        _logger.LogError(
            ex,
            "HTTP error from AI service. Status: {StatusCode}, Model: {ModelId}",
            ex.StatusCode,
            _modelId);

        if (attempt < maxRetries)
        {
          await Task.Delay(delayMs * attempt);
          continue;
        }

        return $"Lỗi kết nối với dịch vụ AI (Mã lỗi: {ex.StatusCode}).\n" +
               "Vui lòng thử lại sau ít phút.";
      }
      catch (TaskCanceledException ex)
      {
        _logger.LogWarning(
            ex,
            "Request timeout (attempt {Attempt}/{MaxRetries}). Model: {ModelId}",
            attempt,
            maxRetries,
            _modelId);

        if (attempt < maxRetries)
        {
          await Task.Delay(delayMs * attempt);
          continue;
        }

        return "⏳ Yêu cầu bị timeout. Vui lòng thử lại với câu hỏi ngắn gọn hơn.";
      }
      catch (Exception ex)
      {
        _logger.LogError(
            ex,
            "Unexpected error in AI chat (attempt {Attempt}/{MaxRetries}). Model: {ModelId}",
            attempt,
            maxRetries,
            _modelId);

        if (attempt < maxRetries)
        {
          await Task.Delay(delayMs * attempt);
          continue;
        }

        return "Đã có lỗi không xác định xảy ra.\n" +
               "Vui lòng thử lại sau hoặc liên hệ hỗ trợ.";
      }
    }

    return "🔌 Không thể kết nối với dịch vụ AI sau nhiều lần thử.\n" +
           "Vui lòng kiểm tra kết nối internet và thử lại sau.";
  }

  /// <summary>
  /// Build chat history với SLIDING WINDOW để giảm token
  /// </summary>
  private ChatHistory BuildOptimizedChatHistory(
      string userMessage,
      List<ChatMessageContent>? history)
  {
    var chatHistory = new ChatHistory();

    // 1. Add MINIMAL system context (KHÔNG chứa data)
    var systemContext = _ragService.BuildMinimalContext();
    chatHistory.AddSystemMessage(systemContext);

    // 2. Add conversation history với SLIDING WINDOW
    if (history != null && history.Count > 0)
    {
      // CHỈ lấy N tin nhắn gần nhất
      var recentHistory = history
          .TakeLast(MAX_HISTORY_MESSAGES)
          .ToList();

      _logger.LogDebug(
          "Adding {Count}/{Total} recent messages to context",
          recentHistory.Count,
          history.Count);

      foreach (var msg in recentHistory)
      {
        chatHistory.Add(msg);
      }
    }

    // 3. Add current user message
    chatHistory.AddUserMessage(userMessage);

    // Log token estimation
    var estimatedTokens = EstimateTokenCount(chatHistory);
    _logger.LogInformation(
        "Estimated tokens: ~{Tokens} (System: ~{SystemTokens}, History: {HistoryCount} msgs, User: 1 msg)",
        estimatedTokens,
        systemContext.Length / 4, // Rough estimate: 1 token ≈ 4 chars
        history?.Count ?? 0);

    return chatHistory;
  }

  /// <summary>
  /// Ước tính số token (rough estimate: 1 token ≈ 4 characters)
  /// </summary>
  private int EstimateTokenCount(ChatHistory chatHistory)
  {
    var totalChars = chatHistory
        .Where(m => m.Content != null)
        .Sum(m => m.Content!.Length);

    return totalChars / 4; // Ước tính thô
  }
}