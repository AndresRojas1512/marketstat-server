namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class ResolvedSalaryFilterDto
{
    public List<int>? LocationIds { get; set; }
    public List<int>? JobIds { get; set; }
    public DateOnly? DateStart { get; set; }
    public DateOnly? DateEnd { get; set; }
}