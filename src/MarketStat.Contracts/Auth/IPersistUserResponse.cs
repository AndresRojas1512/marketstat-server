using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;

namespace MarketStat.Contracts.Auth;

public interface IPersistUserResponse
{
    UserDto User { get; }
}