namespace MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Responses;

public class SalaryDistributionBucket
{
    public decimal LowerBound { get; set; }
    public decimal UpperBound { get; set; }
    public long BucketCount { get; set; }
}