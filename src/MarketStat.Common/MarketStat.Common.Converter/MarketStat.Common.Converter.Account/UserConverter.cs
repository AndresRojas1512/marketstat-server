using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Database.Models.MarketStat.Database.Models.Account;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Account;

public static class UserConverter
{
    public static UserDbModel ToDbModel(User domainUser)
    {
        return new UserDbModel
        {
            UserId = domainUser.UserId,
            Username = domainUser.Username,
            PasswordHash = domainUser.PasswordHash,
            Email = domainUser.Email,
            FullName = domainUser.FullName,
            IsActive = domainUser.IsActive,
            CreatedAt = domainUser.CreatedAt,
            LastLoginAt = domainUser.LastLoginAt,
            SavedBenchmarksCount = domainUser.SavedBenchmarksCount
        };
    }

    public static User ToDomain(UserDbModel dbUser)
    {
        return new User(
            userId: dbUser.UserId,
            username: dbUser.Username,
            passwordHash: dbUser.PasswordHash,
            email: dbUser.Email,
            fullName: dbUser.FullName,
            isActive: dbUser.IsActive,
            createdAt: dbUser.CreatedAt,
            lastLoginAt: dbUser.LastLoginAt,
            savedBenchmarksCount: dbUser.SavedBenchmarksCount
        );
    }
}