namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public sealed class SalaryStatsDto
{
    public long Count { get; set; }
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal Mean { get; set; }
    public decimal Median { get; set; }
    public decimal Percentile25 { get; set; }
    public decimal Percentile75 { get; set; }
}