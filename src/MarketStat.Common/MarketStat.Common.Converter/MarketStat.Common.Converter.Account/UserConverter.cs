using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Database.Models.MarketStat.Database.Models.Account;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Account;

public static class UserConverter
{
    public static UserDbModel ToDbModel(User domainUser)
    {
        if (domainUser == null)
            throw new ArgumentNullException(nameof(domainUser));

        var dbModel = new UserDbModel
        {
            UserId = domainUser.UserId,
            Username = domainUser.Username,
            PasswordHash = domainUser.PasswordHash,
            Email = domainUser.Email,
            FullName = domainUser.FullName,
            IsActive = domainUser.IsActive,
            CreatedAt = domainUser.CreatedAt,
            LastLoginAt = domainUser.LastLoginAt,
            IsAdmin = domainUser.IsAdmin
        };
        return dbModel;
    }

    public static User ToDomain(UserDbModel dbUser)
    {
        if (dbUser == null)
            throw new ArgumentNullException(nameof(dbUser));

        var domainUser = new User(
            userId: dbUser.UserId,
            username: dbUser.Username,
            passwordHash: dbUser.PasswordHash,
            email: dbUser.Email,
            fullName: dbUser.FullName,
            isActive: dbUser.IsActive,
            createdAt: dbUser.CreatedAt,
            lastLoginAt: dbUser.LastLoginAt,
            isAdmin: dbUser.IsAdmin
        );

        if (dbUser.BenchmarkHistories != null && dbUser.BenchmarkHistories.Any())
        {
            domainUser.BenchmarkHistories = dbUser.BenchmarkHistories
                .Select(BenchmarkHistoryConverter.ToDomain)
                .ToList();
        }
        else
        {
            domainUser.BenchmarkHistories = new List<BenchmarkHistory>();
        }

        return domainUser;
    }

    public static List<User> ToDomainList(IEnumerable<UserDbModel> dbUsers)
    {
        if (dbUsers == null)
            return new List<User>();
        return dbUsers.Select(ToDomain).ToList();
    }
}