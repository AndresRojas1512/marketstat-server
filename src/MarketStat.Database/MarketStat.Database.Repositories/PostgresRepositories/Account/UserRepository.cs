using MarketStat.Common.Converter.MarketStat.Common.Converter.Account;
using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Account;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MarketStat.Database.Repositories.PostgresRepositories.Account;

public class UserRepository : IUserRepository
{
    private readonly MarketStatDbContext _dbContext;

    public UserRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<User> AddUserAsync(User user)
    {
        var dbUser = UserConverter.ToDbModel(user);
        _dbContext.Users.Add(dbUser);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx &&
                                           (pgEx.SqlState == PostgresErrorCodes.UniqueViolation))
        {
            _dbContext.Entry(dbUser).State = EntityState.Detached;
            throw new ConflictException("Username or email already exists.");
        }
        return UserConverter.ToDomain(dbUser);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var dbUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username);
        return dbUser == null ? null : UserConverter.ToDomain(dbUser);
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        var dbUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);
        return dbUser == null ? null : UserConverter.ToDomain(dbUser);
    }

    public async Task<bool> UserExistsAsync(string username, string email)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u =>
                u.Username.ToLower() == username.ToLower() ||
                u.Email.ToLower() == email.ToLower());
    }

    public async Task UpdateUserAsync(User user)
    {
        var dbUser = await _dbContext.Users.FindAsync(user.UserId);
        if (dbUser == null)
        {
            throw new NotFoundException($"User with ID {user.UserId} not found for update.");
        }

        dbUser.FullName = user.FullName;
        dbUser.Email = user.Email;
        dbUser.IsActive = user.IsActive;
        dbUser.LastLoginAt = user.LastLoginAt;

        _dbContext.Users.Update(dbUser);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx &&
                                           (pgEx.SqlState == PostgresErrorCodes.UniqueViolation))
        {
            _dbContext.Entry(dbUser).State = EntityState.Detached;
            throw new ConflictException("Update failed due to a conflict (e.g., email already taken by another user).");
        }
    }
}