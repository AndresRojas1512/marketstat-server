namespace MarketStat.Common.Core.Facts.Analytics.Responses;

public class SalarySummary
{
    public decimal? Percentile25 { get; set; }

    public decimal? Percentile50 { get; set; }

    public decimal? Percentile75 { get; set; }

    public decimal? PercentileTarget { get; set; }

    public decimal AverageSalary { get; set; }

    public long TotalCount { get; set; }
}
