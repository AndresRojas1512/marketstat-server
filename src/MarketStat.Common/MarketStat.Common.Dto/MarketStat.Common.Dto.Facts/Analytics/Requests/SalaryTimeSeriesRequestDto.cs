using System.ComponentModel.DataAnnotations;
using MarketStat.Common.Enums;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;

public class SalaryTimeSeriesRequestDto : SalaryFilterDto
{
    public TimeGranularity Granularity { get; set; } = TimeGranularity.Month;

    [Range(0, 120)]
    public int Periods { get; set; } = 12;
}