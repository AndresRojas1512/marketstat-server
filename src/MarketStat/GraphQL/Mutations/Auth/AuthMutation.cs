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

    [Authorize]
    public async Task<UserDto> UpdateProfile(
        PartialUpdateUserDto input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAuthService authService,
        [Service] IMapper mapper)
    {
        var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) 
                           ?? claimsPrincipal.FindFirstValue("nameid");

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
        {
            throw new GraphQLException(new Error("Invalid authentication token.", "AUTH_INVALID_TOKEN"));
        }

        var updatedDomainUser = await authService.PartialUpdateProfileAsync(
            userId,
            input.FullName,
            input.Email
        );

        return mapper.Map<UserDto>(updatedDomainUser);
    }
}