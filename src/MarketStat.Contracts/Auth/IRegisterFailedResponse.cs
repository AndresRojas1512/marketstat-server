namespace MarketStat.Contracts.Auth;

public interface IRegisterFailedResponse
{
    string Reason { get; }
}