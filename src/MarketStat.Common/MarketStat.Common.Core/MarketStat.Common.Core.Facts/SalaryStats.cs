namespace MarketStat.Common.Core.MarketStat.Common.Core.Facts;

public sealed class SalaryStats
{
    public long Count { get; }
    public decimal Min { get; }
    public decimal Max { get; }
    public decimal Mean { get; }
    public decimal Median { get; }
    public decimal Percentile25 { get; }
    public decimal Percentile75 { get; }

    public SalaryStats(long count, decimal min, decimal max, decimal mean, decimal median, decimal p25, decimal p75)
    {
        Count = count;
        Min = min;
        Max = max;
        Mean = mean;
        Median = median;
        Percentile25 = p25;
        Percentile75 = p75;
    }
}