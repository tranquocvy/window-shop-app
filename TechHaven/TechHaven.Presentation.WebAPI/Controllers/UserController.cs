using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechHaven.Application.Features.Users.Commands.UpdateGuideStatus;
using TechHaven.Infrastructure.Authorization;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Users;

namespace TechHaven.Presentation.WebAPI.Controllers;

[Authorize]
public class UserController : BaseApiController
{
  private readonly IMediator _mediator;
  private readonly ILogger<UserController> _logger;

  public UserController(IMediator mediator, ILogger<UserController> logger)
  {
    _mediator = mediator;
    _logger = logger;
  }

  /// <summary>
  /// Update user's guide/onboarding status
  /// </summary>
  /// <remarks>
  /// Allows authenticated users to mark that they have completed the onboarding guide.
  /// Users can only update their own guide status (extracted from JWT token).
  /// </remarks>
  [HttpPut("guide-status")]
  [ProducesResponseType(typeof(ResponseWrapper<bool>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ResponseWrapper<object>), StatusCodes.Status404NotFound)]
  public async Task<IActionResult> UpdateGuideStatus(
      [FromBody] UpdateGuideStatusRequestDto request,
      CancellationToken cancellationToken)
  {
    var currentUserId = User.GetCurrentUserId();

    if (currentUserId == null)
    {
      _logger.LogWarning("Unauthorized attempt to update guide status without valid token");
      return Unauthorized(new ResponseWrapper<object>
      {
        Success = false,
        Message = "Invalid token. User ID not found."
      });
    }

    _logger.LogInformation(
        "User {UserId} updating guide status to {HasSeenGuide}",
        currentUserId.Value,
        request.HasSeenGuide);

    var command = new UpdateGuideStatusCommand(currentUserId.Value, request.HasSeenGuide);
    var result = await _mediator.Send(command, cancellationToken);

    return Ok(new ResponseWrapper<bool>
    {
      Success = true,
      Data = result,
      Message = "Guide status updated successfully"
    });
  }
}