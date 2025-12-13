using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Domain.Common;

namespace TechHaven.Application.Features.Auth.Queries.IsActive;

public record IsActiveQuery(int UserId) : IQuery<IsActiveResponseDto>;