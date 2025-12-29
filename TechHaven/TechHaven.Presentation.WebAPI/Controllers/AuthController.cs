using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Features.Auth.Activate;
using TechHaven.Application.Features.Auth.Login;
using TechHaven.Application.Features.Auth.LoginExternal;
using TechHaven.Application.Features.Auth.Queries.GetCurrentUser;
using TechHaven.Application.Features.Auth.Queries.IsActive;
using TechHaven.Application.Features.Auth.RefreshToken;
using TechHaven.Application.Features.Auth.RefreshTokenExternal;
using TechHaven.Application.Features.Auth.ResendOtp;
using TechHaven.Application.Features.Auth.ResendOtpExternal;
using TechHaven.Application.Features.Auth.Signup;
using TechHaven.Application.Features.Auth.VerifyOtp;
using TechHaven.Application.Features.Auth.VerifyOtpExternal;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly IMediator _mediator;
  private readonly ILogger<AuthController> _logger;

  public AuthController(IMediator mediator, ILogger<AuthController> logger)
  {
    _mediator = mediator;
    _logger = logger;
  }

  /// <summary>
  /// Step 1: Login with username and password. Returns user info and triggers OTP email.
  /// </summary>
  [HttpPost("login")]
  [ProducesResponseType(typeof(ResponseWrapper<LoginResponseDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<ResponseWrapper<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
  {
    try
    {
      _logger.LogInformation("Login attempt for user {UserName}", request.UserName);

      var command = new LoginCommand(request.UserName, request.Password);

      var result = await _mediator.Send(command);

      _logger.LogInformation(
        "Login succeeded for {UserName}. OTP session {SessionId}",
        request.UserName,
        result.OtpSessionId);

      return Ok(new ResponseWrapper<LoginResponseDto>
      {
        Success = true,
        Message = "OTP sent to your email. Please verify to complete login.",
        Data = result
      });
    }
    catch (ValidationException vex)
    {
      _logger.LogWarning(
        vex,
        "Validation failed during login for user {UserName}",
        request.UserName);

      return BadRequest(new ResponseWrapper<object>
      {
        Success = false,
        Message = "Login failed",
        Errors = vex.Errors.SelectMany(kvp => kvp.Value).ToList()
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Login failed for user {UserName}", request.UserName);
      return BadRequest(new ResponseWrapper<object>
      {
        Success = false,
        Message = "Login failed",
        Errors = new List<string> { ex.Message }
      });
    }
  }

    /// <summary>
    /// Step 2: Verify OTP code and receive JWT access token.
    /// </summary>
    [HttpPost("verify-otp")]
  [ProducesResponseType(typeof(ResponseWrapper<OtpVerifyResponseDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<ResponseWrapper<OtpVerifyResponseDto>>> VerifyOtp([FromBody] OtpVerifyRequestDto request)
  {
    try
    {
      _logger.LogInformation(
        "OTP verification attempt for session {SessionId}",
        request.OtpSessionId);

      var command = new VerifyOtpCommand(request.OtpSessionId, request.OtpCode);
      var result = await _mediator.Send(command);

      _logger.LogInformation(
        "OTP verification succeeded for session {SessionId}",
        request.OtpSessionId);

      return Ok(new ResponseWrapper<OtpVerifyResponseDto>
      {
        Success = true,
        Message = "Login successful",
        Data = result
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(
        ex,
        "OTP verification failed for session {SessionId}",
        request.OtpSessionId);

      return BadRequest(new ResponseWrapper<object>
      {
        Success = false,
        Message = "OTP verification failed",
        Errors = new List<string> { ex.Message }
      });
    }
  }

  /// <summary>
  /// Step 3: Refresh access token using refresh token.
  /// </summary>
  [HttpPost("refresh-token")]
  [ProducesResponseType(typeof(ResponseWrapper<RefreshTokenResponseDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<ResponseWrapper<RefreshTokenResponseDto>>> RefreshToken(
      [FromBody] RefreshTokenRequestDto request)
  {
    try
    {
      _logger.LogInformation("Refresh token requested");

      var command = new RefreshTokenCommand(request.RefreshToken);
      var result = await _mediator.Send(command);

      _logger.LogInformation("Refresh token issued successfully");

      return Ok(new ResponseWrapper<RefreshTokenResponseDto>
      {
        Success = true,
        Message = "Token refreshed successfully",
        Data = result
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Refresh token request failed");

      return BadRequest(new ResponseWrapper<object>
      {
        Success = false,
        Message = "Token refresh failed",
        Errors = new List<string> { ex.Message }
      });
    }
  }

  /// <summary>
  /// Resend OTP code to user's email.
  /// </summary>
  [HttpPost("resend-otp")]
  [ProducesResponseType(typeof(ResponseWrapper<OtpResendResponseDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<ResponseWrapper<OtpResendResponseDto>>> ResendOtp(
      [FromBody] OtpResendRequestDto request)
  {
    try
    {
      _logger.LogInformation(
        "OTP resend requested for session {SessionId}",
        request.OtpSessionId);

      var command = new ResendOtpCommand(request.OtpSessionId);
      var result = await _mediator.Send(command);

      _logger.LogInformation(
        "OTP resent successfully for session {SessionId}",
        request.OtpSessionId);

      return Ok(new ResponseWrapper<OtpResendResponseDto>
      {
        Success = true,
        Message = "OTP resent successfully",
        Data = result
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(
        ex,
        "Failed to resend OTP for session {SessionId}",
        request.OtpSessionId);

      return BadRequest(new ResponseWrapper<object>
      {
        Success = false,
        Message = "Failed to resend OTP",
        Errors = new List<string> { ex.Message }
      });
    }
  }

 #region External Auth Flow (BYOD - Bring your own database)

    /// <summary>
    /// Login with external database credentials (BYOD - Bring your own database).
    /// </summary>
    [HttpPost("login-external")]
    public async Task<ActionResult<ResponseWrapper<LoginResponseDto>>> LoginExternal([FromBody] LoginExternalCommand command)
    {
        // Lưu ý: Dùng trực tiếp Command làm Body request để nhanh gọn. 
        // Chuẩn thì nên tạo LoginExternalRequestDto rồi map sang Command.
        try
        {
            _logger.LogInformation("External login attempt for user {UserName} on host {Host}", command.UserName, command.DbHost);
            var result = await _mediator.Send(command);

            // Nếu thành công, trả về token ngay (hoặc flow OTP tùy bạn chọn)
            return Ok(new ResponseWrapper<LoginResponseDto>
            {
                Success = true,
                Message = "OTP sent to email. Please verify with the returned config token.",
                Data = result // Trong result.AccessToken lúc này chứa EncryptedDbConfig
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External login failed");
            return BadRequest(new ResponseWrapper<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Step 2: Verify OTP for external DB login.
    /// </summary>
    [HttpPost("verify-otp-external")]
    [ProducesResponseType(typeof(ResponseWrapper<OtpVerifyResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseWrapper<OtpVerifyResponseDto>>> VerifyOtpExternal([FromBody] VerifyOtpExternalCommand command)
    {
        try
        {
            _logger.LogInformation(
                "OTP verification attempt for session {SessionId}",
                command.OtpSessionId);
            // Lưu ý: Dùng trực tiếp Command làm Body request để nhanh gọn. 
            // Chuẩn thì nên tạo LoginExternalRequestDto rồi map sang Command.

            var result = await _mediator.Send(command);
            _logger.LogInformation(
           "OTP verification succeed for session {SessionId}",
           command.OtpSessionId);
            return Ok(new ResponseWrapper<OtpVerifyResponseDto>
            {
                Success = true,
                Message = "External login successful.",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
               "OTP verification failed for session {SessionId}",
               command.OtpSessionId);
            return BadRequest(new ResponseWrapper<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Resend OTP for external DB login flow.
    /// </summary>
    [HttpPost("resend-otp-external")]
    [ProducesResponseType(typeof(ResponseWrapper<OtpResendResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseWrapper<OtpResendResponseDto>>> ResendOtpExternal([FromBody] ResendOtpExternalCommand command)
    {
        try
        {
            _logger.LogInformation(
              "OTP resend requested for session {SessionId}",
              command.OtpSessionId);
            var result = await _mediator.Send(command);
            return Ok(new ResponseWrapper<OtpResendResponseDto>
            {
                Success = true,
                Message = "OTP resent successfully.",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to resend OTP for session {SessionId}",
                command.OtpSessionId);
            return BadRequest(new ResponseWrapper<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Step 3: Refresh access token for external database users
    /// </summary>
    [HttpPost("refresh-token-external")]
    [ProducesResponseType(typeof(ResponseWrapper<RefreshTokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResponseWrapper<RefreshTokenResponseDto>>> RefreshTokenExternal(
        [FromBody] RefreshTokenExternalCommand command)
    {
        try
        {
            _logger.LogInformation("Refresh token external requested");
            var result = await _mediator.Send(command);

            _logger.LogInformation("Refresh token external issued successfully");

            return Ok(new ResponseWrapper<RefreshTokenResponseDto>
            {
                Success = true,
                Message = "Token refreshed successfully",
                Data = result
            });
        }
        catch (ValidationException vex)
        {
            _logger.LogError(vex, "Refresh token external validation failed");
            return BadRequest(new ResponseWrapper<object>
            {
                Success = false,
                Message = "Token refresh failed",
                Errors = vex.Errors.SelectMany(kvp => kvp.Value).ToList()
            });
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh token external request failed");

            return BadRequest(new ResponseWrapper<object>
            {
                Success = false,
                Message = "Token refresh failed",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    #endregion


    #region Common Features (Used for both Default & External via Middleware)
    /// <summary>
    /// Get current user information from access token
    /// </summary>
    /// <returns>User information</returns>
    [HttpGet("me")]
  [Authorize] // Yêu cầu access token hợp lệ
  [ProducesResponseType(typeof(ResponseWrapper<UserInfoDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
  {
    _logger.LogInformation(
      "Fetching current user profile for principal {User}",
      User.Identity?.Name ?? "anonymous");

    var query = new GetCurrentUserQuery();
    var result = await _mediator.Send(query, cancellationToken);

    if (!result.IsSuccess)
    {
      _logger.LogWarning(
        "Current user lookup failed: {Reason}",
        result.ErrorMessage ?? "Unknown");

      return Unauthorized(new ResponseWrapper<object>
      {
        Success = false,
        Message = result.ErrorMessage ?? "Unauthorized"
      });
    }

    _logger.LogInformation("Current user lookup succeeded for {User}", result.Data?.UserName);

    return Ok(new ResponseWrapper<UserInfoDto>
    {
      Success = true,
      Message = "User information retrieved successfully.",
      Data = result.Data
    });
  }

  /// <summary>
  /// Register a new user account
  /// </summary>
  [HttpPost("signup")]
  [ProducesResponseType(typeof(ResponseWrapper<SignupResponseDto>), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<ResponseWrapper<SignupResponseDto>>> Signup(
    [FromBody] SignupRequestDto request,
    CancellationToken cancellationToken)
  {
    try
    {
      _logger.LogInformation(
        "Signup attempt for username {UserName}, email {Email}",
        request.UserName,
        request.Email);

      var command = new SignupCommand(
        request.UserFullName,
        request.Email,
        request.UserName,
        request.Password,
      // request.ConfirmPassword
        request.RoleId
      );

      var result = await _mediator.Send(command, cancellationToken);

      _logger.LogInformation(
        "Signup succeeded for user {UserName}. UserId: {UserId}",
        result.UserName,
        result.UserId);

      return StatusCode(StatusCodes.Status201Created, new ResponseWrapper<SignupResponseDto>
      {
        Success = true,
        Message = "Account created successfully",
        Data = result
      });
    }
    catch (ValidationException vex)
    {
      _logger.LogWarning(
        vex,
        "Validation failed during signup for username {UserName}",
        request.UserName);

      return BadRequest(new ResponseWrapper<object>
      {
        Success = false,
        Message = "Signup failed",
        Errors = vex.Errors.SelectMany(kvp => kvp.Value).ToList()
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(
        ex,
        "Signup failed for username {UserName}",
        request.UserName);

      return BadRequest(new ResponseWrapper<object>
      {
        Success = false,
        Message = "Signup failed",
        Errors = new List<string> { ex.Message }
      });
    }
  }
    /// <summary>
    /// Check user active status and remaining trial days
    /// </summary>
    [HttpGet("isActive")]
    [Authorize] // Bắt buộc phải có Token để biết check cho ai
    [ProducesResponseType(typeof(ResponseWrapper<IsActiveResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResponseWrapper<IsActiveResponseDto>>> IsActive(CancellationToken cancellationToken)
    {
        // Lấy UserId từ Token (sử dụng logic ClaimTypes.NameIdentifier)
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(new ResponseWrapper<object> { Success = false, Message = "Invalid Token" });
        }

        var query = new IsActiveQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ResponseWrapper<IsActiveResponseDto>
        {
            Success = true,
            Data = result
        });
    }

    /// <summary>
    /// Activate user account with a key code
    /// </summary>
    [HttpPost("activate")]
    [Authorize] // Bắt buộc phải có Token
    [ProducesResponseType(typeof(ResponseWrapper<ActivateResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResponseWrapper<ActivateResponseDto>>> Activate(
        [FromBody] ActivateRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Lấy UserId từ Token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new ResponseWrapper<object> { Success = false, Message = "Invalid Token" });
            }

            var command = new ActivateCommand(userId, request.Key);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsValid)
            {
                return BadRequest(new ResponseWrapper<object>
                {
                    Success = false,
                    Message = "Invalid activation key."
                });
            }

            return Ok(new ResponseWrapper<ActivateResponseDto>
            {
                Success = true,
                Message = "Account activated successfully.",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Activation failed");
            return BadRequest(new ResponseWrapper<object>
            {
                Success = false,
                Message = "Activation failed",
                Errors = new List<string> { ex.Message }
            });
        }
    }
    #endregion
}