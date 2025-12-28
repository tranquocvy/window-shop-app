using AutoMapper;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.AppSettings;

namespace TechHaven.Application.Features.AppSetting.Queries.GetAppSettingByKey;

public class GetAppSettingByKeyQueryHandler : IQueryHandler<GetAppSettingByKeyQuery, Result<AppSettingDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAppSettingByKeyQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AppSettingDto>> Handle(
        GetAppSettingByKeyQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.AppSettings.GetSettingAsync(
            request.Key,
            request.CurrentUserId, //find by user id and key
            cancellationToken);

        if (entity == null)
        {
            return Result<AppSettingDto>.Failure(
                $"App Setting with Key '{request.Key}' not found.",
                ErrorType.NotFound);
        }

        var dto = _mapper.Map<AppSettingDto>(entity);

        // Đảm bảo UserId và IsSystem được set đúng
        dto.UserId = entity.UserId;
        dto.IsSystem = entity.UserId == null;

        return Result<AppSettingDto>.Success(dto);
    }
}