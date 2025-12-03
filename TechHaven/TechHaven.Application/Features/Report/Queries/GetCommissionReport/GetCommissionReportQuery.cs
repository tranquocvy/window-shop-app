using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.Reports;

namespace TechHaven.Application.Features.Reports.Queries.GetCommissionReport;

public record GetCommissionReportQuery(
  int Month,
  int Year
) : IQuery<Result<List<CommissionReportDto>>>;