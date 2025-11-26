namespace MarketStat.Contracts.Auth;

public interface ILoginRequest
{
    string Username { get; }
    string Password { get; }
}