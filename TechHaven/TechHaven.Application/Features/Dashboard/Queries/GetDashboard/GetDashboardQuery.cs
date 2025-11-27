using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.Dashboard;

namespace TechHaven.Application.Features.Dashboard.Queries.GetDashboard;

/// <summary>
/// Query to get dashboard overview data
/// </summary>
public record GetDashboardQuery : IQuery<Result<DashboardDto>>;