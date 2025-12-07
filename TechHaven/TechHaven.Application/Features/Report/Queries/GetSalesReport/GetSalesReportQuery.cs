using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.Reports;

namespace TechHaven.Application.Features.Reports.Queries.GetSalesReport;

public record GetSalesReportQuery(
  DateTime StartDate,
  DateTime EndDate,
  ReportPeriodType PeriodType
) : IQuery<Result<List<SalesReportDto>>>;