using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.Reports;

namespace TechHaven.Application.Features.Reports.Queries.GetTopSellingProducts;

public record GetTopSellingProductsQuery(
  DateTime StartDate,
  DateTime EndDate,
  int TopCount = 10
) : IQuery<Result<List<ProductSalesDto>>>;