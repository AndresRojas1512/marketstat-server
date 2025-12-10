namespace MarketStat.Contracts.Auth;

public interface IGetUserAuthDetailsRequest
{
    string Username { get; }
}