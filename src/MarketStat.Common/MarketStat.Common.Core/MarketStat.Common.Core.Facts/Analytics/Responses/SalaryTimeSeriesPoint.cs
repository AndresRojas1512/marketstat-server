namespace MarketStat.Common.Core.Facts.Analytics.Responses;

public class SalaryTimeSeriesPoint
{
    public DateOnly PeriodStart { get; set; }

    public decimal AvgSalary { get; set; }

    public long SalaryCountInPeriod { get; set; }
}
