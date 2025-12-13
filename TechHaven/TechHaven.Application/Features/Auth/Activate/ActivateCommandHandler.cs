using Microsoft.Extensions.Logging;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.Activate;

public class ActivateCommandHandler : ICommandHandler<ActivateCommand, ActivateResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivateCommandHandler> _logger;

    // Key kích hoạt cứng theo yêu cầu
    private const string HARDCODED_ACTIVATION_KEY = "4252640910";

    public ActivateCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ActivateCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ActivateResponseDto> Handle(
        ActivateCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Kiểm tra Key có đúng không
        if (request.Key != HARDCODED_ACTIVATION_KEY)
        {
            _logger.LogWarning("User {UserId} attempted activation with invalid key.", request.UserId);
            return new ActivateResponseDto { IsValid = false };
        }

        // 2. Tìm User
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        // 3. Kích hoạt user
        user.IsActive = true;

        // 4. Lưu thay đổi
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} activated successfully.", request.UserId);

        return new ActivateResponseDto { IsValid = true };
    }
}