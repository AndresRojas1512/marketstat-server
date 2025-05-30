namespace MarketStat.Common.Core.MarketStat.Common.Core.Account;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public int SavedBenchmarksCount { get; set; }
    public bool IsEtlUser { get; set; }

    public virtual ICollection<BenchmarkHistory> BenchmarkHistories { get; set; }
    
    public User()
    {
        Username = string.Empty;
        PasswordHash = string.Empty;
        Email = string.Empty;
        FullName = string.Empty;
        BenchmarkHistories = new List<BenchmarkHistory>();
        CreatedAt = DateTimeOffset.UtcNow;
        IsActive = true;
        IsEtlUser = false;
    }

    public User(int userId, string username, string passwordHash, string email, string fullName,
        bool isActive, DateTimeOffset createdAt, DateTimeOffset? lastLoginAt, int savedBenchmarksCount, bool isEtlUser)
    {
        UserId = userId;
        Username = username ?? throw new ArgumentNullException(nameof(username));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
        IsActive = isActive;
        CreatedAt = createdAt;
        LastLoginAt = lastLoginAt;
        SavedBenchmarksCount = savedBenchmarksCount;
        IsEtlUser = isEtlUser;
        BenchmarkHistories = new List<BenchmarkHistory>();
    }
}