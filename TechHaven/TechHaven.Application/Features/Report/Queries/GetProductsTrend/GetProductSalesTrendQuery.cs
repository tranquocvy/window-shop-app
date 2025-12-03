using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.Reports;

namespace TechHaven.Application.Features.Reports.Queries.GetProductsTrend;

public record GetProductSalesTrendQuery(
  int ProductId,
  DateTime StartDate,
  DateTime EndDate,
  ReportPeriodType PeriodType
) : IQuery<Result<ProductSalesTrendDto>>;