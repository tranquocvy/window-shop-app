using AutoMapper;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.AppSettings;

namespace TechHaven.Application.Features.AppSetting.Commands.CreateAppSetting;

public class CreateAppSettingCommandHandler : ICommandHandler<CreateAppSettingCommand, Result<AppSettingDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateAppSettingCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AppSettingDto>> Handle(
        CreateAppSettingCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            int? targetUserId = request.IsSystem ? null : request.CurrentUserId;

            // Validate Business Logic: Key uniqueness

            bool exists = await _unitOfWork.AppSettings.AnyAsync(
                x => x.Key == request.Key && x.UserId == targetUserId,
                cancellationToken);

            if (exists)
            {
                string scope = request.IsSystem ? "System" : "User";
                return Result<AppSettingDto>.Failure(
                    $"Setting with key '{request.Key}' already exists for {scope}.",
                    ErrorType.Conflict);
            }

            // Map Command to Entity
            var entity = new Domain.Entities.AppSetting
            {
                Key = request.Key,
                Value = request.Value,
                ValueType = request.ValueType,
                Category = request.Category,
                Description = request.Description,
                UserId = targetUserId, 
            };

            // Save to DB
            await _unitOfWork.AppSettings.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to DTO to return
            var dto = _mapper.Map<AppSettingDto>(entity);

            // Handle IsSystem for returned DTO (since Entity does not have IsSystem column)
            dto.IsSystem = entity.UserId == null;
            dto.UserId = entity.UserId;

            return Result<AppSettingDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<AppSettingDto>.Failure(
                $"Failed to create app setting: {ex.Message}",
                ErrorType.InternalError);
        }
    }
}