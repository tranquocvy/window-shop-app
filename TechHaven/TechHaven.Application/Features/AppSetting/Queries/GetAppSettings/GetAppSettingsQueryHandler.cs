using AutoMapper;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.SearchCriteria;
using TechHaven.Shared.DTOs.AppSettings;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Application.Features.AppSetting.Queries.GetAppSettings;

public class GetAppSettingsQueryHandler : IQueryHandler<GetAppSettingsQuery, Result<PagingResponse<AppSettingDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAppSettingsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagingResponse<AppSettingDto>>> Handle(
        GetAppSettingsQuery request,
        CancellationToken cancellationToken)
    {
        // prepare Criteria
        var criteria = new AppSettingSearchCriteria
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchKeyword,
            UserId = request.CurrentUserId
        };

        var (entities, totalCount) = await _unitOfWork.AppSettings.SearchWithPaginationAsync(criteria, cancellationToken);

        var dtos = _mapper.Map<List<AppSettingDto>>(entities);

        // Set IsSystem and UserId for each DTO
        for (int i = 0; i < dtos.Count; i++)
        {
            dtos[i].IsSystem = entities[i].UserId == null;
            dtos[i].UserId = entities[i].UserId;
        }

        // Response wrapping
        var response = new PagingResponse<AppSettingDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagingResponse<AppSettingDto>>.Success(response);
    }
}