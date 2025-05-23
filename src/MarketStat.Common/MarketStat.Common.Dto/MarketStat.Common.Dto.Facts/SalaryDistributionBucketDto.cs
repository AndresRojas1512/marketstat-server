namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class SalaryDistributionBucketDto
{
    public decimal LowerBound { get; set; }
    public decimal UpperBound { get; set; }
    public long BucketCount { get; set; }
}