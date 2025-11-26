using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;

namespace MarketStat.Contracts.Facts.Analytics;

public interface IGetFactSalaryTimeSeriesRequest
{
    SalaryTimeSeriesRequestDto Filter { get; }
}