using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;

namespace MarketStat.Contracts.Auth;

public interface IRegisterResponse
{
    bool Success { get; }
    string Message { get; }
    UserDto User { get; }
}