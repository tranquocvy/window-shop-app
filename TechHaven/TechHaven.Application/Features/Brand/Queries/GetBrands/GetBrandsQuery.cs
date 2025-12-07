using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.Brands;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Application.Features.Brand.Queries.GetBrandsQuery;

public record GetBrandsQuery(
  string searchTerm,
  bool? inStockOnly,
  string? sortBy,
  bool sortDescending = false
) : IQuery<Result<List<BrandDto>>>;