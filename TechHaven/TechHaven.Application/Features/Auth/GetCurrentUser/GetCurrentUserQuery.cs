using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Domain.Common;

namespace TechHaven.Application.Features.Auth.Queries.GetCurrentUser;

/// <summary>
/// Query to get current user information from access token
/// </summary>
public record GetCurrentUserQuery : IQuery<Result<UserInfoDto>>;