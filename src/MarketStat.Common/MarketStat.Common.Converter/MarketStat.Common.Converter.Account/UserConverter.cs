using MarketStat.Common.Core.Account;
using MarketStat.Database.Models.Account;

namespace MarketStat.Common.Converter.Account;

public static class UserConverter
{
    public static UserDbModel ToDbModel(User domainUser)
    {
        ArgumentNullException.ThrowIfNull(domainUser);

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
            IsAdmin = domainUser.IsAdmin,
        };
        return dbModel;
    }

    public static User ToDomain(UserDbModel dbUser)
    {
        ArgumentNullException.ThrowIfNull(dbUser);

        var domainUser = new User(
            userId: dbUser.UserId,
            username: dbUser.Username,
            passwordHash: dbUser.PasswordHash,
            email: dbUser.Email,
            fullName: dbUser.FullName,
            isActive: dbUser.IsActive,
            createdAt: dbUser.CreatedAt,
            lastLoginAt: dbUser.LastLoginAt,
            isAdmin: dbUser.IsAdmin);

        return domainUser;
    }

    public static IList<User> ToDomainList(IEnumerable<UserDbModel> dbUsers)
    {
        if (dbUsers == null)
        {
            return new List<User>();
        }

        return dbUsers.Select(ToDomain).ToList();
    }
}
