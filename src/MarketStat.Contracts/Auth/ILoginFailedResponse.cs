namespace MarketStat.Contracts.Auth;

public interface ILoginFailedResponse
{
    string Reason { get; }
}