using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;

namespace MarketStat.Services.Auth.AuthService;

public interface IAuthService
{
    Task<User> RegisterAsync(string username, string password, string email, string fullname, bool isAdmin);
    Task<AuthResult> LoginAsync(string username, string password);
    Task<User> PartialUpdateProfileAsync(int userId, string? fullName, string? email);
    Task<User> GetUserProfileAsync(int userId);
}