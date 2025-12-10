using DocumentFormat.OpenXml.Office2010.Excel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechHaven.Application.Features.AppSetting.Commands.CreateAppSetting;
using TechHaven.Application.Features.AppSetting.Commands.UpdateAppSetting;
using TechHaven.Application.Features.AppSetting.Queries.GetAppSettingByKey;
using TechHaven.Application.Features.AppSetting.Queries.GetAppSettings;
using TechHaven.Infrastructure.Authorization;
using TechHaven.Infrastructure.Authorization.Handlers;
using TechHaven.Shared.DTOs.AppSettings;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Presentation.WebAPI.Controllers;

[Authorize]
public class AppSettingController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<AppSettingController> _logger;
    private readonly IAuthorizationService _authorizationService;

    public AppSettingController(
        IMediator mediator,
        ILogger<AppSettingController> logger,
        IAuthorizationService authorizationService)
    {
        _mediator = mediator;
        _logger = logger;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// GET api/Setting
    /// Get list app settings (with paging and filter). 
    /// Automatically filter by current user id || system app setting 
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ManageSettings)]
    [ProducesResponseType(typeof(ResponseWrapper<PagingResponse<AppSettingDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppSettings(
        [FromQuery] AppSettingsQueryDto request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetCurrentUserId();
        if (currentUserId == null)
        {
            return Unauthorized(new ResponseWrapper<object>
            {
                Success = false,
                Message = "Invalid token. User ID not found."
            });
        }

        var query = new GetAppSettingsQuery
        {
            CurrentUserId = (int)currentUserId,
            SearchKeyword = request.SearchKeyword ?? string.Empty
        };

        _logger.LogInformation(
            "User {UserId} ({Role}) fetching settings. Page: {Page}",
            currentUserId, User.GetCurrentUserRole(), query.PageNumber);

        var result = await _mediator.Send(query, cancellationToken);

        // LOG: Output summary (Tìm thấy bao nhiêu?)
        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Found {Count} settings for User {UserId}",
                result.Data?.Items.Count ?? 0, currentUserId);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// GET api/AppSetting/{key}
    /// Get an app setting by key and user id
    /// If not found, return System setting default
    /// </summary>
    [HttpGet("{key}")]
    [Authorize(Policy = AuthorizationPolicies.ManageSettings)]
    [ProducesResponseType(typeof(ResponseWrapper<AppSettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppSettingById(
        string key,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetCurrentUserId();
        if (currentUserId == null)
        {
            return Unauthorized(new ResponseWrapper<object>
            {
                Success = false,
                Message = "Invalid token. User ID not found."
            });
        }

        _logger.LogInformation(
            "User {UserId} fetching setting '{Key}'",
            currentUserId, key);

        var query = new GetAppSettingByKeyQuery(key)
        {
            CurrentUserId = currentUserId.Value
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess)
        {
            // Authorization check: User chỉ được xem setting của mình hoặc system
            var settingUserId = result.Data!.IsSystem ? null : (int?)currentUserId.Value;
            var authContext = new AppSettingAuthorizationContext
            {
                TargetUserId = settingUserId,
                Key = key
            };

            var authResult = await _authorizationService.AuthorizeAsync(
                User,
                authContext,
                "AppSettingAccess");

            if (!authResult.Succeeded)
            {
                _logger.LogWarning(
                    "User {UserId} unauthorized to access setting '{Key}'",
                    currentUserId, key);

                return StatusCode(StatusCodes.Status403Forbidden, new ResponseWrapper<object>
                {
                    Success = false,
                    Message = "User unauthorized to access setting."
                });
            }

            _logger.LogInformation(
                "Setting '{Key}' found. IsSystem: {IsSystem}",
                key, result.Data!.IsSystem);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// POST api/Setting
    /// create new application setting
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ManageSettings)]
    [ProducesResponseType(typeof(ResponseWrapper<AppSettingDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAppSetting(
        [FromBody] AppSettingUpsertRequestDto request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetCurrentUserId();
        if (currentUserId == null)
        {
            return Unauthorized(new ResponseWrapper<object>
            {
                Success = false,
                Message = "Invalid token. User ID not found."
            });
        }

        // Authorization: Chỉ Admin mới được tạo System Setting
        if (request.IsSystem && !User.IsAdmin())
        {
            _logger.LogWarning(
                "User {UserId} (non-Admin) attempted to create System Setting '{Key}'",
                currentUserId, request.Key);

            return StatusCode(StatusCodes.Status403Forbidden, new ResponseWrapper<object>
            {
                Success = false,
                Message = "Only Admin can create System Settings."
            });
        }

        _logger.LogInformation(
            "User {UserId} creating setting '{Key}'. IsSystem: {IsSystem}",
            currentUserId, request.Key, request.IsSystem);

        var command = new CreateAppSettingCommand
        {
            CurrentUserId = currentUserId.Value,
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
    [Authorize(Policy = AuthorizationPolicies.ManageSettings)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAppSetting(
        string key,
        [FromBody] AppSettingUpsertRequestDto request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetCurrentUserId();
        if (currentUserId == null)
        {
            return Unauthorized(new ResponseWrapper<object>
            {
                Success = false,
                Message = "Invalid token. User ID not found."
            });
        }

        // Authorization check: Seller không được update System Setting
        if (request.IsSystem && !User.IsAdmin())
        {
            _logger.LogWarning(
                "User {UserId} (non-Admin) attempted to update System Setting '{Key}'",
                currentUserId, key);

            return StatusCode(StatusCodes.Status403Forbidden, new ResponseWrapper<object>
            {
                Success = false,
                Message = "Only Admin can update System Settings."
            });
        }

        var targetUserId = request.IsSystem ? null : (int?)currentUserId.Value;
        var authContext = new AppSettingAuthorizationContext
        {
            TargetUserId = targetUserId,
            Key = key
        };

        var authResult = await _authorizationService.AuthorizeAsync(
            User,
            authContext,
            "AppSettingAccess");

        if (!authResult.Succeeded)
        {
            _logger.LogWarning(
                "User {UserId} unauthorized to update setting '{Key}'",
                currentUserId, key);

            return Forbid();
        }

        _logger.LogInformation(
            "User {UserId} updating setting '{Key}'",
            currentUserId, key);

        var command = new UpdateAppSettingCommand
        {
            CurrentUserId = currentUserId.Value,
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