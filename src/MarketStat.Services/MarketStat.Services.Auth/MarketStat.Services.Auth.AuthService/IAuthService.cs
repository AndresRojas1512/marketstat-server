using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;

namespace MarketStat.Services.Auth.AuthService;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterUserDto registerDto);
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto loginDto);
}