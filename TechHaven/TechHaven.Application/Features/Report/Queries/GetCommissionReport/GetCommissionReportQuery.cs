using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.Reports;

namespace TechHaven.Application.Features.Reports.Queries.GetCommissionReport;

public record GetCommissionReportQuery(
  DateTime StartDate,
  DateTime EndDate,
  int? UserId = null
) : IQuery<Result<List<CommissionReportDto>>>;