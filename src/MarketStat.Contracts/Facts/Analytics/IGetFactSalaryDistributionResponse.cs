using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Payloads;

namespace MarketStat.Contracts.Facts.Analytics;

public interface IGetFactSalaryDistributionResponse
{
    List<SalaryDistributionBucketDto> Buckets { get;  }
}
