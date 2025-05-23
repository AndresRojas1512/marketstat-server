namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;

public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreateAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public int SavedBenchmarksCount { get; set; }
}