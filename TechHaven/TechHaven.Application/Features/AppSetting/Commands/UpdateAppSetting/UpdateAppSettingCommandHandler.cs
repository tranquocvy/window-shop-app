using AutoMapper;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.AppSettings;
using Microsoft.EntityFrameworkCore; // Cần để dùng FirstOrDefaultAsync

namespace TechHaven.Application.Features.AppSetting.Commands.UpdateAppSetting;

public class UpdateAppSettingCommandHandler : ICommandHandler<UpdateAppSettingCommand, Result<AppSettingDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateAppSettingCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AppSettingDto>> Handle(
        UpdateAppSettingCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            int? targetUserId = request.IsSystem ? null : request.CurrentUserId;

            //  Tìm bản ghi cần update
            // Không dùng _unitOfWork.AppSettings.GetByKeyAsync vì hàm đó có logic fallback (ưu tiên User -> System).

            var setting = await _unitOfWork.AppSettings.FirstOrDefaultAsync(
                s => s.Key == request.Key && s.UserId == targetUserId,
                cancellationToken);

            if (setting == null)
            {
                string scope = request.IsSystem ? "System" : "User";
                return Result<AppSettingDto>.Failure(
                    $"Setting '{request.Key}' not found for {scope}. If you want to create a new override, use Create/POST.",
                    ErrorType.NotFound);
            }

            // Chỉ update những trường có giá trị (hoặc update tất cả tùy business rule)
            setting.Value = request.Value;
            setting.Category = request.Category;
            setting.Description = request.Description;

            // Nếu entity có trường UpdatedAt thì cập nhật
             setting.UpdatedAt = DateTime.UtcNow; 

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            
            var dto = _mapper.Map<AppSettingDto>(setting);
            dto.IsSystem = setting.UserId == null;
            dto.UserId = setting.UserId;

            return Result<AppSettingDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<AppSettingDto>.Failure(
                $"Failed to update app setting: {ex.Message}",
                ErrorType.InternalError);
        }
    }
}