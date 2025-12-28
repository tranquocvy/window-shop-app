using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Domain.Common;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.Entities;
using TechHaven.Application.Common.Exceptions;

namespace TechHaven.Application.Features.Auth.Queries.IsActive;

public class IsActiveQueryHandler : IQueryHandler<IsActiveQuery, IsActiveResponseDto>
{
	private readonly IUnitOfWork _unitOfWork;
	
	private const int TRIAL_PERIOD_DAYS = 15;

	public IsActiveQueryHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<IsActiveResponseDto> Handle(
		IsActiveQuery request,
		CancellationToken cancellationToken)
	{
		var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);

		if (user == null)
		{
			throw new NotFoundException(nameof(User), request.UserId);
		}

		// Logic
		// Nếu user đã Active -> DaysRemain có thể trả về -1 hoặc số ngày đã dùng tùy ý (ở đây để max int hoặc logic tùy biến)
		// Nếu chưa Active -> Tính xem đã tạo account bao lâu rồi.

		var daysUsed = (DateTime.UtcNow - user.CreatedAt).Days;
		var daysRemain = TRIAL_PERIOD_DAYS - daysUsed;

		// Đảm bảo không trả về số âm nếu đã quá hạn
		if (daysRemain < 0) daysRemain = 0;

		return new IsActiveResponseDto
		{
			IsActive = user.IsActive,
			DaysRemain = daysRemain
		};
	}
}