using Microsoft.AspNetCore.Mvc;
using TechHaven.Domain.Common;
using TechHaven.Infrastructure.Authorization;
using TechHaven.Shared.DTOs.Common;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
  // Helper method để convert Result -> IActionResult
  protected IActionResult HandleResult<T>(Result<T> result)
  {
    if (result.IsSuccess)
    {
      return Ok(new ResponseWrapper<T>
      {
        Success = true,
        Data = result.Data
      });
    }

    // Map error code to HTTP status
    var statusCode = result.ErrorCode switch
    {
      ErrorType.NotFound => StatusCodes.Status404NotFound,
      ErrorType.Validation => StatusCodes.Status400BadRequest,
      ErrorType.Conflict => StatusCodes.Status409Conflict,
      ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
      ErrorType.Forbidden => StatusCodes.Status403Forbidden,
      _ => StatusCodes.Status500InternalServerError
    };

    var errors = result.Errors.Any()
        ? result.Errors
        : new List<string> { result.ErrorMessage ?? "Unknown error" };

    return StatusCode(statusCode, new ResponseWrapper<T>
    {
      Success = false,
      Message = result.ErrorMessage ?? "Operation failed",
      Errors = errors
    });
  }

  // Overload cho non-generic Result
  protected IActionResult HandleResult(Result result)
  {
    if (result.IsSuccess)
    {
      return Ok(new ResponseWrapper<object>
      {
        Success = true,
        Message = "Operation completed successfully"
      });
    }

    var statusCode = result.ErrorCode switch
    {
      ErrorType.NotFound => StatusCodes.Status404NotFound,
      ErrorType.Validation => StatusCodes.Status400BadRequest,
      ErrorType.Conflict => StatusCodes.Status409Conflict,
      _ => StatusCodes.Status500InternalServerError
    };

    return StatusCode(statusCode, new ResponseWrapper<object>
    {
      Success = false,
      Message = result.ErrorMessage ?? "Operation failed",
      Errors = result.Errors.Any()
            ? result.Errors
            : new List<string> { result.ErrorMessage ?? "Unknown error" }
    });
  }

  // Overload cho CreatedAtAction
  protected IActionResult HandleResult<T>(Result<T> result, string actionName, object routeValues)
  {
    if (result.IsSuccess)
    {
      return CreatedAtAction(actionName, routeValues, new ResponseWrapper<T>
      {
        Success = true,
        Data = result.Data
      });
    }

    // Map error code to HTTP status
    var statusCode = result.ErrorCode switch
    {
      ErrorType.NotFound => StatusCodes.Status404NotFound,
      ErrorType.Validation => StatusCodes.Status400BadRequest,
      ErrorType.Conflict => StatusCodes.Status409Conflict,
      ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
      ErrorType.Forbidden => StatusCodes.Status403Forbidden,
      _ => StatusCodes.Status500InternalServerError
    };

    var errors = result.Errors.Any()
        ? result.Errors
        : new List<string> { result.ErrorMessage ?? "Unknown error" };

    return StatusCode(statusCode, new ResponseWrapper<T>
    {
      Success = false,
      Message = result.ErrorMessage ?? "Operation failed",
      Errors = errors
    });
  }

  protected int GetCurrentUserIdOrThrow()
  {
    var userId = User.GetCurrentUserId();
    if (userId == null)
    {
      throw new UnauthorizedAccessException("User ID not found in token");
    }
    return userId.Value;
  }
}