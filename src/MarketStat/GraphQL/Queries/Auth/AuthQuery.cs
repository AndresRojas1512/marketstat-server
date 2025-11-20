using System.Security.Claims;
using AutoMapper;
using HotChocolate.Authorization;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;
using MarketStat.Database.Core.Repositories.Account;
using MarketStat.Services.Auth.AuthService;

namespace MarketStat.GraphQL.Queries.Auth;

[ExtendObjectType("Query")]
public class AuthQuery
{
    [Authorize]
    public async Task<UserDto> Me(
        ClaimsPrincipal claimsPrincipal,
        [Service] IAuthService authService,
        [Service] IMapper mapper)
    {
        var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) 
                           ?? claimsPrincipal.FindFirstValue("nameid");
        
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
        {
            throw new GraphQLException(new Error("Invalid authentication token. User ID missing.", "AUTH_INVALID_TOKEN"));
        }

        var domainUser = await authService.GetUserProfileAsync(userId);

        return mapper.Map<UserDto>(domainUser);
    }
}