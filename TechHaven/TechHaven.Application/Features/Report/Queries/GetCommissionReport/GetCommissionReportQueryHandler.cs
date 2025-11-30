using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Reports;

namespace TechHaven.Application.Features.Reports.Queries.GetCommissionReport;

public class GetCommissionReportQueryHandler
    : IQueryHandler<GetCommissionReportQuery, Result<List<CommissionReportDto>>>
{
  private readonly IOrderRepository _orderRepository;

  public GetCommissionReportQueryHandler(IOrderRepository orderRepository)
  {
    _orderRepository = orderRepository;
  }

  public async Task<Result<List<CommissionReportDto>>> Handle(
      GetCommissionReportQuery request,
      CancellationToken cancellationToken)
  {
    try
    {
      var report = await _orderRepository.GetCommissionReportAsync(
        request.StartDate,
        request.EndDate,
        request.UserId,
        cancellationToken
      );

      var topSellerDtos = report.Select(p => new CommissionReportDto
      {
        UserId = p.UserId,
        UserFullName = p.UserFullName,
        RoleName = p.RoleName,
        TotalSales = p.TotalSales,
        CommissionRate = p.CommissionRate,
        CommissionAmount = p.CommissionAmount,
        TotalOrders = p.TotalOrders,
      }).ToList();

      return Result<List<CommissionReportDto>>.Success(topSellerDtos);
    }
    catch (Exception ex)
    {
      return Result<List<CommissionReportDto>>.Failure(
        $"Failed to get commission report: {ex.Message}",
        ErrorType.InternalError
      );
    }
  }
}