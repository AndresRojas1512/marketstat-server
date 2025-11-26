namespace MarketStat.Contracts.Auth;

public interface ILoginResponse
{
    string Token { get; }
    DateTime Expiration { get; }
    string Username { get; }
    string Email { get; }
    bool IsActive { get; }
}