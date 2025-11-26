namespace MarketStat.Contracts.Auth;

public interface IPersistUserCommand
{
    string Username { get; }
    string PasswordHash { get; }
    string Email { get; }
    string FullName { get; }
    bool IsAdmin { get; }
}