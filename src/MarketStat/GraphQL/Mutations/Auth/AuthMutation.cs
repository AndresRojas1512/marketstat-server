using System.Security.Claims;
using AutoMapper;
using HotChocolate.Authorization;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;
using MarketStat.Services.Auth.AuthService;

namespace MarketStat.GraphQL.Mutations.Auth;

[ExtendObjectType("Mutation")]
public class AuthMutation
{
    public async Task<UserDto> RegisterUser(RegisterUserDto input, [Service] IAuthService authService,
        [Service] IMapper mapper)
    {
        var domainUser = await authService.RegisterAsync(input.Username, input.Password, input.Email, input.FullName,
            input.IsAdmin);
        return mapper.Map<UserDto>(domainUser);
    }

    public async Task<AuthResponseDto> LoginUser(LoginRequestDto input, [Service] IAuthService authService,
        [Service] IMapper mapper)
    {
        var authResult = await authService.LoginAsync(input.Username, input.Password);
        return new AuthResponseDto
        {
            Token = authResult.Token,
            Expiration = authResult.Expiration,
            User = mapper.Map<UserDto>(authResult.User)
        };
    }
}