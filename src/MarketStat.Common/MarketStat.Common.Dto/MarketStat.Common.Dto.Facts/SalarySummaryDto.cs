namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class SalarySummaryDto
{
    public decimal? Percentile25 { get; set; }
    public decimal? Percentile50 { get; set; }
    public decimal? Percentile75 { get; set; }
    public decimal? PercentileTarget { get; set; }
    public decimal? AverageSalary { get; set; }
    public long TotalCount { get; set; }

    public SalarySummaryDto()
    {
        TotalCount = 0;
    }
}