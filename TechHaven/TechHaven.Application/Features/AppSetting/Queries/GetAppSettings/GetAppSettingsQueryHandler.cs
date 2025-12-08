using AutoMapper;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
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

        // 4. Xử lý logic hiển thị bổ sung (Optional)
        // Ví dụ: Đánh dấu IsSystem trong DTO
        //foreach (var dto in dtos)
        //{
            // Nếu entity gốc có UserId null -> Là System Setting
            // (Lưu ý: Logic này nên làm trong AutoMapper Profile thì gọn hơn)
            // dto.IsSystem = ... (Đã xử lý trong Mapper ở bước trước)
        //}

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