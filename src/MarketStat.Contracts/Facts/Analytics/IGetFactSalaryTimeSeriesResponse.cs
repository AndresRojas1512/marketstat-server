using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Payloads;

namespace MarketStat.Contracts.Facts.Analytics;

public interface IGetFactSalaryTimeSeriesResponse
{
    List<SalaryTimeSeriesPointDto> Points { get; }
}