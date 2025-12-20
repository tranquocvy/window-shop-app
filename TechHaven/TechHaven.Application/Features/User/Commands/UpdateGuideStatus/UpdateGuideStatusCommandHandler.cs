using Microsoft.Extensions.Logging;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Application.Features.Users.Commands.UpdateGuideStatus;

public class UpdateGuideStatusCommandHandler : ICommandHandler<UpdateGuideStatusCommand, bool>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<UpdateGuideStatusCommandHandler> _logger;

  public UpdateGuideStatusCommandHandler(
      IUnitOfWork unitOfWork,
      ILogger<UpdateGuideStatusCommandHandler> logger)
  {
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  public async Task<bool> Handle(UpdateGuideStatusCommand request, CancellationToken cancellationToken)
  {
    _logger.LogInformation(
        "Updating HasSeenGuide status for User {UserId} to {HasSeenGuide}",
        request.UserId,
        request.HasSeenGuide);

    var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

    if (user == null)
    {
      _logger.LogWarning("User {UserId} not found", request.UserId);
      throw new NotFoundException($"User with ID {request.UserId} not found");
    }

    // Update guide status
    user.HasSeenGuide = request.HasSeenGuide;

    await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation(
        "Successfully updated HasSeenGuide to {HasSeenGuide} for User {UserId}",
        request.HasSeenGuide,
        request.UserId);

    return true;
  }
}