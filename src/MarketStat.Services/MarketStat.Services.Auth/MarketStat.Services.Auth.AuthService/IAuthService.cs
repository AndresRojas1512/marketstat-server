namespace MarketStat.Services.Auth.AuthService;

using MarketStat.Common.Dto.Account.User;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterUserDto registerDto);

    Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto);

    Task<UserDto> PartialUpdateProfileAsync(int userId, string? fullName, string? email);
}
