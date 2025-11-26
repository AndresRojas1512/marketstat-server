using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Payloads;

namespace MarketStat.Contracts.Facts.Analytics;

public interface IGetFactSalarySummaryResponse
{
    SalarySummaryDto Summary { get; }
}