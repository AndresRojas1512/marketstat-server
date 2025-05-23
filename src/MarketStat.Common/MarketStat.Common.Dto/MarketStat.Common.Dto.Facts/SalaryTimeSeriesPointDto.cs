namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class SalaryTimeSeriesPointDto
{
    public DateOnly PeriodStart { get; set; }
    public decimal? AvgSalary { get; set; }
}