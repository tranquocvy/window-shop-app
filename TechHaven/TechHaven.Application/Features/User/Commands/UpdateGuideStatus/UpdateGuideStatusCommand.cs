using TechHaven.Application.Interfaces;

namespace TechHaven.Application.Features.Users.Commands.UpdateGuideStatus;

public record UpdateGuideStatusCommand(int UserId, bool HasSeenGuide) : ICommand<bool>;