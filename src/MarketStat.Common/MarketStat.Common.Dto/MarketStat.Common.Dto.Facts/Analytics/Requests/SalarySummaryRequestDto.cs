using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;

public class SalarySummaryRequestDto : SalaryFilterDto
{
    [Range(0, 100)]
    public int TargetPercentile { get; set; } = 90;
}