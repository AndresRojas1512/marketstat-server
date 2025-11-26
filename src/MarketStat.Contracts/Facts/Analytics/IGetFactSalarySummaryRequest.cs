using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;

namespace MarketStat.Contracts.Facts.Analytics;

public interface IGetFactSalarySummaryRequest
{
    SalarySummaryRequestDto Filter { get; }
}