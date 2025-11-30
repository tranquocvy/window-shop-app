using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.Reports;

namespace TechHaven.Application.Features.Reports.Queries.GetSalesTrend;

public record GetSalesTrendQuery(
  DateTime StartDate,
  DateTime EndDate,
  ReportPeriodType PeriodType
) : IQuery<Result<SalesTrendDto>>;