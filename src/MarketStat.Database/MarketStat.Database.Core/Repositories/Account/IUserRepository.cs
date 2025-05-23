using MarketStat.Common.Core.MarketStat.Common.Core.Account;

namespace MarketStat.Database.Core.Repositories.Account;

public interface IUserRepository
{
    Task<User> AddUserAsync(User user);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByIdAsync(int userId);
    Task<bool> UserExistsAsync(string username, string email);
    Task UpdateUserAsync(User user);
}