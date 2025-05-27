using System.ComponentModel.DataAnnotations;
using MarketStat.Common.Enums;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class BenchmarkQueryDto
{
    public int? IndustryFieldId { get; set; }
    public int? StandardJobRoleId { get; set; }
    public int? HierarchyLevelId { get; set; }
    public int? DistrictId { get; set; }
    public int? OblastId { get; set; }
    public int? CityId { get; set; }

    public DateOnly? DateStart { get; set; }
    public DateOnly? DateEnd { get; set; }
    
    [Range(0, 100, ErrorMessage = "Target percentile must be between 0 and 100.")]
    public int TargetPercentile { get; set; } = 90;

    public TimeGranularity Granularity { get; set; } = TimeGranularity.Month;

    [Range(1, int.MaxValue, ErrorMessage = "Periods must be a positive integer.")]
    public int Periods { get; set; } = 12;
}