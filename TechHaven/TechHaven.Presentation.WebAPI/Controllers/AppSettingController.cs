using DocumentFormat.OpenXml.Office2010.Excel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechHaven.Application.Features.AppSetting.Commands.CreateAppSetting;
using TechHaven.Application.Features.AppSetting.Commands.UpdateAppSetting;
using TechHaven.Application.Features.AppSetting.Queries.GetAppSettingByKey;
using TechHaven.Application.Features.AppSetting.Queries.GetAppSettings;
using TechHaven.Shared.DTOs.AppSettings;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Presentation.WebAPI.Controllers;

//[Authorize] -> Tạm bỏ qua để test API
public class AppSettingController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<AppSettingController> _logger;

    public AppSettingController(IMediator mediator, ILogger<AppSettingController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Helper: Get User id from Access Token
    /// </summary>
    private int GetUserIdFromToken()
    {
        // Lấy tạm userId = 0 để test API
        //Nếu chỉ cần test API thì nên comment nội dung của hàm này và chỉ return về 0
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");

        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        _logger.LogWarning("Security Alert: Could not extract UserID from Token in AppSettingController.");
        return 0;
    }

    /// <summary>
    /// GET api/Setting
    /// Get list app settings (with paging and filter). 
    /// Automatically filter by current user id || system app setting 
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseWrapper<PagingResponse<AppSettingDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppSettings(
        [FromQuery] GetAppSettingsQuery query, 
        CancellationToken cancellationToken)
    {
        query.CurrentUserId = GetUserIdFromToken();

        _logger.LogInformation(
            "Fetching settings for User {UserId}. Page: {Page}, Search: {Search}",
            query.CurrentUserId, query.PageNumber, query.SearchKeyword ?? "None");

        var result = await _mediator.Send(query, cancellationToken);
        // LOG: Output summary (Tìm thấy bao nhiêu?)
        if (result.IsSuccess)
        {
            _logger.LogInformation("Found {Count} settings.", result.Data?.Items.Count ?? 0);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// GET api/AppSetting/{key}
    /// Get an app setting by key and user id
    /// If not found, return System setting default
    /// </summary>
    [HttpGet("{key}")]
    [ProducesResponseType(typeof(ResponseWrapper<AppSettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppSettingById(
        string key,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetUserIdFromToken();

        _logger.LogInformation("Fetching AppSetting detail Key: {Key} for User: {UserId}", key, currentUserId);

        var query = new GetAppSettingByKeyQuery(key)
        {
            CurrentUserId = currentUserId
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("AppSetting Key '{Key}' not found.", key);
        }
        else
        {
            _logger.LogInformation("Found setting '{Key}'. IsSystem: {IsSystem}", key, result.Data!.IsSystem);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// POST api/Setting
    /// create new application setting
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResponseWrapper<AppSettingDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAppSetting(
        [FromBody] AppSettingUpsertRequestDto request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetUserIdFromToken();
        _logger.LogInformation("User {UserId} creating setting key '{Key}'", currentUserId, request.Key);

        var command = new CreateAppSettingCommand
        {
            CurrentUserId = currentUserId,
            Key = request.Key,
            Value = request.Value,
            ValueType = (Domain.Enums.SettingType)request.ValueType,
            Category = request.Category,
            Description = request.Description,
            IsSystem = request.IsSystem // True: Tạo cho hệ thống, False: Tạo riêng cho User
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            // LOG: Success (Tạo xong ID bao nhiêu)
            _logger.LogInformation("Setting created successfully. Key: {Key}, ID: {Id}", result.Data?.Key, result.Data?.AppSettingId);
            var response = new ResponseWrapper<AppSettingDto>
            {
                Success = true,
                Data = result.Data,
                Message = "Setting created successfully"
            };
            return StatusCode(StatusCodes.Status201Created, response);
        }
        // LOG: Failure
        _logger.LogWarning("Failed to create setting '{Key}'. Error: {Error}", request.Key, result.ErrorMessage);

        return HandleResult(result);
    }

    /// <summary>
    /// PUT api/Setting/{key}
    /// Update current an app setting name <key>  with userid from token
    /// </summary>
    [HttpPut("{key}")]
    [ProducesResponseType(typeof(ResponseWrapper<AppSettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAppSetting(
        string key,
        [FromBody] AppSettingUpsertRequestDto request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetUserIdFromToken();
        _logger.LogInformation("User {UserId} updating setting key '{appSettingId}'", currentUserId, key);

        
        var command = new UpdateAppSettingCommand
        {
            CurrentUserId = currentUserId,
            Key = key,
            Value = request.Value,
            Category = request.Category,
            Description = request.Description,
            IsSystem = request.IsSystem
        };

        var result = await _mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }
}