namespace MarketStat.Contracts.Auth;

public interface ISubmitRegisterCommand
{
    string Username { get; }
    string Password { get; }
    string Email { get; }
    string FullName { get; }
    bool IsAdmin { get; }
}