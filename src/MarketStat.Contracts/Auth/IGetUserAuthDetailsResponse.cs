namespace MarketStat.Contracts.Auth;

public interface IGetUserAuthDetailsResponse
{
    int UserId { get; }
    string Username { get; }
    string PasswordHash { get; }
    string Email { get; }
    bool IsActive { get; }
    bool IsAdmin { get; }
}