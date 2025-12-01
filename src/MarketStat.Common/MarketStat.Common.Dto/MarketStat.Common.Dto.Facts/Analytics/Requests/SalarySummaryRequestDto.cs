namespace MarketStat.Common.Dto.Facts.Analytics.Requests;

using System.ComponentModel.DataAnnotations;

public class SalarySummaryRequestDto : SalaryFilterDto
{
    [Range(0, 100)]
    public int TargetPercentile { get; set; } = 90;
}
