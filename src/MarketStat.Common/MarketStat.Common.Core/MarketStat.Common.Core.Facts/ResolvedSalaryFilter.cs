namespace MarketStat.Common.Core.MarketStat.Common.Core.Facts;

public class ResolvedSalaryFilter
{
    public List<int>? LocationIds { get; set; }
    public List<int>? JobIds { get; set; }
    public DateOnly? DateStart { get; set; }
    public DateOnly? DateEnd { get; set; }
}