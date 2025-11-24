using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Application.Features.Auth.Login;
using TechHaven.Application.Features.Auth.VerifyOtp;
using TechHaven.Application.Features.Auth.RefreshToken;
using TechHaven.Application.Features.Auth.ResendOtp;
using TechHaven.Application.Features.Auth.Queries.GetCurrentUser;

namespace TechHaven.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly IMediator _mediator;

  public AuthController(IMediator mediator)
  {
    _mediator = mediator;
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
      var command = new LoginCommand(request.UserName, request.Password);

      var result = await _mediator.Send(command);

      return Ok(new ResponseWrapper<LoginResponseDto>
      {
        Success = true,
        Message = "OTP sent to your email. Please verify to complete login.",
        Data = result
      });
    }
    catch (Exception ex)
    {
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
      var command = new VerifyOtpCommand(request.OtpSessionId, request.OtpCode);
      var result = await _mediator.Send(command);

      return Ok(new ResponseWrapper<OtpVerifyResponseDto>
      {
        Success = true,
        Message = "Login successful",
        Data = result
      });
    }
    catch (Exception ex)
    {
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
      var command = new RefreshTokenCommand(request.RefreshToken);
      var result = await _mediator.Send(command);

      return Ok(new ResponseWrapper<RefreshTokenResponseDto>
      {
        Success = true,
        Message = "Token refreshed successfully",
        Data = result
      });
    }
    catch (Exception ex)
    {
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
      var command = new ResendOtpCommand(request.OtpSessionId);
      var result = await _mediator.Send(command);

      return Ok(new ResponseWrapper<OtpResendResponseDto>
      {
        Success = true,
        Message = "OTP resent successfully",
        Data = result
      });
    }
    catch (Exception ex)
    {
      return BadRequest(new ResponseWrapper<object>
      {
        Success = false,
        Message = "Failed to resend OTP",
        Errors = new List<string> { ex.Message }
      });
    }
  }

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
    var query = new GetCurrentUserQuery();
    var result = await _mediator.Send(query, cancellationToken);

    if (!result.IsSuccess)
    {
      return Unauthorized(new ResponseWrapper<object>
      {
        Success = false,
        Message = result.ErrorMessage ?? "Unauthorized"
      });
    }

    return Ok(new ResponseWrapper<UserInfoDto>
    {
      Success = true,
      Message = "User information retrieved successfully.",
      Data = result.Data
    });
  }
}