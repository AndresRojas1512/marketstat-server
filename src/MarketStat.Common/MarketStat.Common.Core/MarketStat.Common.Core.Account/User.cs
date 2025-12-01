namespace MarketStat.Common.Core.Account;

public class User
{
    public User()
    {
        Username = string.Empty;
        PasswordHash = string.Empty;
        Email = string.Empty;
        FullName = string.Empty;
        CreatedAt = DateTimeOffset.UtcNow;
        IsActive = true;
        IsAdmin = false;
    }

    public User(
        int userId,
        string username,
        string passwordHash,
        string email,
        string fullName,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset? lastLoginAt,
        bool isAdmin)
    {
        UserId = userId;
        Username = username ?? throw new ArgumentNullException(nameof(username));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
        IsActive = isActive;
        CreatedAt = createdAt;
        LastLoginAt = lastLoginAt;
        IsAdmin = isAdmin;
    }

    public int UserId { get; set; }

    public string Username { get; set; }

    public string PasswordHash { get; set; }

    public string Email { get; set; }

    public string FullName { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    public bool IsAdmin { get; set; }
}
