using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.Reports;

namespace TechHaven.Application.Features.Reports.Queries.GetDashboardSummary;

public record GetDashboardSummaryQuery() : IQuery<Result<DashboardSummaryDto>>;