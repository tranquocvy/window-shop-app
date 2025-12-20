using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Domain.Common;

namespace TechHaven.Application.Features.Auth.Activate;

/// <summary>
/// Command for activating a user account.
/// </summary>
public record ActivateCommand(int UserId, string Key) : ICommand<ActivateResponseDto>;