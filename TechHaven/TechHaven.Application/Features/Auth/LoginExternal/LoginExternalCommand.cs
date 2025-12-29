using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.LoginExternal;

// Request nhận vào thông tin kết nối DB và thông tin đăng nhập
public record LoginExternalCommand(
    string DbHost,
    string DbPort,
    string DbName,
    string DbUser,
    string DbPass,
    string UserName,
    string Password
) : ICommand<LoginResponseDto>;