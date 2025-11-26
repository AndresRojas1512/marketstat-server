using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;

namespace MarketStat.Contracts.Facts.Analytics;

public interface IGetPublicRolesRequest
{
    PublicRolesRequestDto Filter { get; }
}