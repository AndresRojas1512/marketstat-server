using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Payloads;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;

namespace MarketStat.Contracts.Facts.Analytics;

public interface IGetFactSalaryDistributionRequest
{
    SalaryFilterDto Filter { get; }
}
